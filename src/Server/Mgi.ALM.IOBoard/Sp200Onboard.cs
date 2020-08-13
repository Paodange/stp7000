using Polly;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mgi.ALM.Util.Extension;

namespace Mgi.ALM.IOBoard
{
    public class Sp200Onboard : IOnboardHardware
    {

        #region com send/recv  value
        public const byte HEADER1 = 0x55;
        public const byte HEADER2 = 0xAA;
        private enum FrameType : byte
        {
            LAUNCH_RESPONSE_RESULT = 0x01,
            RESPONSE = 0x02,
            RESULT = 0x03,
            LAUNCH_RESPONSE = 0x04,
        };
        public enum ObjectUnit : byte
        {
            LAUNCH_UNIT = 0x0C,
            RESPONSE = 0x01
        };
        public enum CommandCode : byte
        {
            LAMINAR_HOOD = 0x01,
            SAFETY_LOCK = 0x02,
            FLOODLIGHT = 0x03,
            STERILAMP = 0x04,
            LED_LIGHT = 0x05,
            FRONT_DOOR_SWITCH = 0x06,
            PCR_ELECTRIC_SOURCE = 0x07,
            BUZZER = 0x08,
            GET_VERSION = 0xE0
        };
        public enum ErrorTypes : byte
        {
            CORRECT = 0x00,
            TIME_OUT = 0x01,
            WRONG_FRAME_TYPE = 0x02,
            WRONG_OBJECT_UNIT = 0x03,
            REPEAT_FRAME = 0x04,
        };
        public enum FunctionCode : byte
        {
            Device_OFF = 0x00,
            Device_ON = 0x01,
            BUZZER_SET_RATE = 0x02,
            BUZZER_SET_TIME = 0x03,
            BUZZER_GET_VOLUME = 0x04
        }
        public enum States : byte
        {
            NORMAL_RESULT_FRAME = 0x00,
            EXCEPTION_RESULT_FRAME = 0x01,
        };
        #endregion


        #region variable
        /// <summary>
        /// 打开的端口
        /// </summary>
        private SerialPort port;
        private SensorConfig _config;
        private bool _opened = false;
        private bool _inited = false;

        private ushort _frameIndex = 0;
        private ushort FrameIndex
        {
            get
            {
                lock (MyLock1)
                {
                    if (_frameIndex >= ushort.MaxValue)
                        _frameIndex = ushort.MinValue;
                    return _frameIndex++;
                }
            }
        }
        private readonly object MyLock = new object();
        private readonly object MyLock1 = new object();
        private event EventHandler<byte[]> _revDataArrive;
        private List<byte> _buff = new List<byte>();
        private Task _revDataTask;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        #endregion

        #region struct method
        public Sp200Onboard(SensorConfig config)
        {
            Crc.crcInit();
            _config = config;
        }
        #endregion

        #region public methods
        public void Close()
        {
            if (port != null)
            {
                try
                {
                    GpioOut((uint)CommandCode.BUZZER, 0);
                }
                catch
                {
                    //do nothing
                }
                try
                {
                    if (_revDataTask != null && _revDataTask.Status == TaskStatus.Running)
                    {
                        _tokenSource.Cancel();
                        _revDataTask.Wait();
                    }
                }
                catch (Exception ex)
                {
                }

                try
                {
                    port.Close();
                }
                catch
                {

                }
            }
        }

        public void Initialize()
        {
            GpioOut((uint)CommandCode.BUZZER, 1);
            Thread.Sleep(100);
            GpioOut((uint)CommandCode.BUZZER, 0);
            _inited = true;
        }

        public void Open()
        {
            port = new SerialPort(_config.ComName, _config.BaudRate, Parity.None, 8, StopBits.One);
            try
            {
                port.Open();
                _revDataTask = new Task(() => RevData(), _tokenSource.Token);
                _revDataTask.Start();
            }
            catch (Exception ex)
            {
                throw new SensorException($"Open {_config.ComName} error：{ex.Message}", ex);
            }
            _opened = true;
        }

        public void ReOpen()
        {
            if (_opened)
                return;
            Open();
        }

        public void ReInitialize()
        {
            if (_inited)
                return;
            Initialize();
        }


        /// <summary>
        /// 查询视窗状态
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public uint GpioIn(uint socket)
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData((CommandCode)socket);
                arryList.Add(1);
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            uint frontdoorState = 0;
            Action<byte[]> _getDoorState = argus =>
            {
                frontdoorState = (uint)argus[11];
            };

            CommandOfCommon(_createFrame, new { SwitchState = 1 }, (CommandCode)socket, _getDoorState);
            return frontdoorState;
        }

        public void ReloadConfiguration()
        {
            return;
        }

        public void GpioOut(uint socket, uint switchState)
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData((CommandCode)socket);
                arryList.Add((byte)switchState);
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            CommandOfCommon(_createFrame, new { SwitchState = switchState }, (CommandCode)socket, null);
        }

        public int GetVolume()
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.BUZZER);
                arryList.Add((byte)FunctionCode.BUZZER_GET_VOLUME);
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            int volume = 0;
            Action<byte[]> _getVolume = argus =>
            {
                volume = argus.ToList<byte>().GetRange(11, 4).ToArray().LittleEndianToInt32();
            };

            CommandOfCommon(_createFrame, new { Function = FunctionCode.BUZZER_GET_VOLUME.ToString() }, CommandCode.BUZZER, _getVolume);

            return volume;
        }

        /// <summary>
        /// 设置下层流罩速度，下层流罩没有使能开关，设置0速度代表关闭
        /// </summary>
        /// <param name="speed">0-1000</param>
        public void HoodSpeed(int speed)
        {
            if (speed < 0 || speed > 1000)
            {
                throw new SensorException($"HoodSpeed({speed}) out of range [0-1000]");
            }

            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.LAMINAR_HOOD);
                //02表示设置下层流罩速度，00与01代表上层流罩开关
                arryList.Add(0x02);
                arryList.AddRange(speed.LittleEndianToByteArray());
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            CommandOfCommon(_createFrame, new { Speed = speed }, CommandCode.LAMINAR_HOOD, null);
        }

        public void Color(byte r, byte g, byte b)
        {

            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.LED_LIGHT);
                arryList.Add(0x03);
                arryList.AddRange(new byte[] { r, g, b });
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            CommandOfCommon(_createFrame, new { R = r, G = g, B = b }, CommandCode.LED_LIGHT, null);
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="duration"></param>
        public void SetBuzzerDuration(int duration)
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.BUZZER);
                arryList.Add((byte)FunctionCode.BUZZER_SET_TIME);
                arryList.AddRange(((ushort)duration).LittleEndianToByteArray());
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            CommandOfCommon(_createFrame, new { Voice = duration }, CommandCode.BUZZER, null);
        }

        /// <summary>
        /// 设置频率
        /// </summary>
        /// <param name="baud"></param>
        public void SetBuzzerBaud(int baud)
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.BUZZER);
                arryList.Add((byte)FunctionCode.BUZZER_SET_RATE);
                arryList.AddRange(((ushort)baud).LittleEndianToByteArray());
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            CommandOfCommon(_createFrame, new { Baud = baud }, CommandCode.BUZZER, null);
        }

        public string GetVersion()
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.GET_VERSION);
                arryList.Add(0x01);
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            string version = string.Empty;
            Action<byte[]> _getVersion = argus =>
            {
                version = $"{(int)argus[11] }.{(int)argus[12] }.{(int)argus[13] }."
                                + $"{(int)(argus[14] | argus[15] << 8 | argus[16] << 16 | argus[17] << 24)  }."
                                + $"{(int)argus[18] }.{(int)argus[19] }.{(int)argus[20] }.{(int)argus[21] }";
            };
            CommandOfCommon(_createFrame, null, CommandCode.GET_VERSION, _getVersion);
            return version;
        }

        public void FlashColor(byte r, byte g, byte b, ushort onTime, ushort offTime)
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.LED_LIGHT);
                arryList.Add(0x04);
                arryList.AddRange(new byte[] { r, g, b });
                arryList.AddRange((onTime.LittleEndianToByteArray()));
                arryList.AddRange((offTime.LittleEndianToByteArray()));
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            CommandOfCommon(_createFrame, new { R = r, G = g, B = b, onTime, offTime }, CommandCode.LED_LIGHT, null);
        }

        public void FlashBuzzer(ushort onTime, ushort offTime)
        {
            Func<byte[]> _createFrame = () =>
            {
                List<byte> arryList = FillCommonSendData(CommandCode.BUZZER);
                arryList.Add(0x05);
                arryList.Add(0x01);
                arryList.AddRange((onTime.LittleEndianToByteArray()));
                arryList.AddRange((offTime.LittleEndianToByteArray()));
                var sendBytes = CreateFrame(arryList.ToArray());
                return sendBytes;
            };

            CommandOfCommon(_createFrame, new { onTime, offTime }, CommandCode.BUZZER, null);
        }


        public void Execute(Action action, object info, string methodName)
        {
            lock (MyLock)
            {
                var result = Policy.HandleResult<bool>(false)
                              .WaitAndRetry(3, n => TimeSpan.FromSeconds(1))
                              .ExecuteAndCapture(() =>
                              {
                                  try
                                  {
                                      action.Invoke();
                                  }
                                  catch (Exception ex)
                                  {
                                      return false;
                                  }
                                  return true;
                              });

                if (!result.Result)
                {
                    throw new SensorException($"{methodName} retry 3 times, but still fail");
                }
            }
        }
        #endregion


        /// <summary>
        /// 提取出公共部分，不一样的部分（body中间数据部分，responseID响应ID，action处理接收数据）
        /// </summary>
        /// <param name="body"></param>
        /// <param name="responseID"></param>
        /// <param name="action"></param>
        private void CommandOfCommon(Func<byte[]> createFrame, object info, CommandCode responseID, Action<byte[]> action)
        {

            int count = 0;
            bool compelete = false;

            EventHandler<byte[]> _listen = (sender, argus) =>
            {
                if (argus[8] == (byte)responseID)
                {
                    //_logger.Info($"Method: {responseID.ToString()}, receive data: {argus.ToHexString()}");
                    if (ReceiveDataCheck(argus, responseID))
                        count++;
                }
                if (argus[4] == (byte)FrameType.RESULT && argus[8] == (byte)responseID)
                {
                    //处理接收数据
                    action?.Invoke(argus);
                };
                if (count >= 2)
                {
                    compelete = true;
                }
            };

            Action _sendMsg = () =>
            {
                try
                {
                    _revDataArrive += _listen;
                    var sendBytes = createFrame();
                    Send(sendBytes);
                    var begin = DateTime.UtcNow;
                    var result = (this as object).WaitForEnd(TimeSpan.FromSeconds(_config.RevTimeOut), TimeSpan.FromMilliseconds(50),
                        () => compelete);

                    if (!result)
                    {
                        //_logger.Error($"{responseID.ToString()}: receive data timeout({_config.RevTimeOut}s)");
                        throw new SensorException($"-0xFF[1430730]0xEE-{responseID.ToString()}: receive data timeout({_config.RevTimeOut}s)");
                    }
                    _revDataArrive -= _listen;
                }
                catch (Exception ex)
                {
                    _revDataArrive -= _listen;
                    throw ex;
                }
            };
            Execute(_sendMsg, info, responseID.ToString());
        }

        /// <summary>
        /// 校验接收数据的crc,响应状态
        /// </summary>
        /// <param name="revBytes"></param>
        /// <returns></returns>
        private bool ReceiveDataCheck(byte[] revBytes, CommandCode responseID)
        {
            bool result = true;
            try
            {
                byte[] newData = new byte[revBytes.Length - 6];
                Array.Copy(revBytes, 4, newData, 0, revBytes.Length - 6);
                if (Crc.crcFast(newData, newData.Length) != revBytes.ToList<byte>().GetRange(revBytes.Length - 2, 2).ToArray().LittleEndianToUshort())
                {
                    throw new SensorException($"{responseID.ToString()}: Receive data lose:crc error");
                }
                switch (revBytes[4])
                {
                    case (byte)FrameType.RESPONSE:
                        if (revBytes[9] != (byte)ErrorTypes.CORRECT)
                        {
                            throw new SensorException($"{responseID.ToString()}: IO board response error");
                        }
                        break;
                    case (byte)FrameType.RESULT:
                        if (revBytes[9] == (byte)States.EXCEPTION_RESULT_FRAME)
                        {
                            throw new SensorException($"{responseID.ToString()}: IO board result error");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        #region private methods
        /// <summary>
        /// 收集协议不一样部分的数据，到这里整合成完整的协议数据返回
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        private byte[] CreateFrame(byte[] byteArray)
        {

            List<byte> arryList = new List<byte>();
            arryList.Add(HEADER1);
            arryList.Add(HEADER2);
            arryList.AddRange(((ushort)byteArray.Length).LittleEndianToByteArray());
            arryList.AddRange(byteArray);
            ushort crc = Crc.crcFast(byteArray, byteArray.Length);
            arryList.AddRange(crc.LittleEndianToByteArray());

            return arryList.ToArray();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sendBytes"></param>
        private void Send(byte[] sendBytes)
        {
            try
            {
                port.Write(sendBytes, 0, sendBytes.Length);
            }
            catch (Exception ex)
            {
                throw new SensorException($"IO board send data error:{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 填充一帧数据中公用的数据部分
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private List<byte> FillCommonSendData(CommandCode code)
        {
            List<byte> arryList = new List<byte>();
            arryList.Add((byte)FrameType.LAUNCH_RESPONSE_RESULT);
            arryList.AddRange(FrameIndex.LittleEndianToByteArray());
            arryList.Add((byte)ObjectUnit.LAUNCH_UNIT);
            arryList.Add((byte)code);
            return arryList;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Receive();
        }

        private void RevData()
        {
            while (true)
            {
                if (_tokenSource.Token.IsCancellationRequested)
                    break;
                Thread.Sleep(100);
                try
                {
                    byte[] revBytes = new byte[port.BytesToRead];
                    port.Read(revBytes, 0, revBytes.Length);

                    _buff.AddRange(revBytes);

                    while (true)
                    {
                        if (_buff.Count > 6)
                        {
                            var frame = GetOneFrame(_buff);
                            if (frame == null)
                            {
                                break;
                            }
                            else
                            {
                                OnFrameArrived(frame);
                                _buff.RemoveRange(0, frame.Length);
                            }
                        }
                        else
                            break;
                    }
                }

                catch (Exception ex)
                {
                    _buff.Clear();
                    Thread.Sleep(30);
                }
            }
        }

        private byte[] GetOneFrame(List<byte> buff)
        {
            if (buff[0] != HEADER1 || buff[1] != HEADER2)
            {
                _buff.RemoveAt(0);
                return null;
            }
            //

            var length = (buff[2] | buff[3] << 8) + 6;
            if (buff.Count >= length)
            {
                return new List<byte>(buff.GetRange(0, length)).ToArray();
            }
            else
                return null;
        }

        /// <summary>
        /// 验证数据，通过验证过转发数据
        /// </summary>
        /// <param name="frame"></param>
        private void OnFrameArrived(byte[] frame)
        {
            _revDataArrive?.Invoke(this, frame);
        }

        public void Reset()
        {
            //nothing
        }

        public void Stop()
        {
            //nothing
        }


        #endregion

    }
}
