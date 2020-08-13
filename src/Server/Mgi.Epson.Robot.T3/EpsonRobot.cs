using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mgi.Epson.Robot.T3.Extentions;

namespace Mgi.Epson.Robot.T3
{
    public class EpsonRobot : IEpsonRobot
    {
        /// <summary>
        /// 头部开始
        /// </summary>
        private const string cmdHead = "#head";

        /// <summary>
        /// 尾部结束
        /// </summary>
        private const string cmdTail = "#tail";

        /// <summary>
        /// 方法与参数分隔符
        /// </summary>
        private const string methodSeparator = ":";

        /// <summary>
        /// 参数与参数之间分隔符
        /// </summary>
        private const string paramsSeparator = ",";
        #region 变量
        private TcpClient _tcp;
        private NetworkStream _streamToServer;

        private Task _revDataTask;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private event EventHandler<string> _oneFrameArrive;
        private string _rcvDatas;

        /// <summary>
        /// 配置
        /// </summary>
        private EpsonRobotConfig _config;
        private readonly object MyLock = new object();
        private EpsonEthernetClient _ethernet;
        #endregion

        public EpsonRobot(EpsonRobotConfig config)
        {
            _config = config;

            _ethernet = new EpsonEthernetClient(_config);
        }

        #region public method
        public void Open()
        {
            _tcp = new TcpClient();
            try
            {
                if (_ethernet.TcpClient == null || !_ethernet.TcpClient.Connected)
                {
                    _ethernet.Open();
                }
                _ethernet.Login();
                _ethernet.Reset();
                Thread.Sleep(1000);
                _ethernet.StartMain();
                Thread.Sleep(1000);
                _tcp.Connect(_config.EpsonIP, _config.EpsonPort);

                _streamToServer = _tcp.GetStream();

                _tokenSource = new CancellationTokenSource();
                _revDataTask = new Task(() => RevData(), _tokenSource.Token);
                _revDataTask.Start();


                PowerLow();
            }
            catch (Exception ex)
            {
                throw new Exception($"Connect fail:{ex.Message}", ex);
            }
        }
        public void ReOpen()
        {
            Open();
        }
        public void Close()
        {
            try
            {
                if (_revDataTask?.Status == TaskStatus.Running)
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
                _tcp?.Close();
                _tcp?.Dispose();
                _revDataTask = null;
                _ethernet?.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Tcp close fail:{ex.Message}", ex);
            }
        }
        public void Initialize()
        {
            Reset();
            //ExcuteScript("home");
            return;
        }
        public void ReInitialize()
        {
            Initialize();
        }

        public void MotorOff()
        {
            var oneFrame = FillOneFrame(FunctionType.MotorOff, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.MotorOff, null);
        }
        public void MotorOn()
        {
            var oneFrame = FillOneFrame(FunctionType.MotorOn, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.MotorOn, null);
        }

        public void Go(int pointNum, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0)
        {
            var oneFrame = FillOneFrame(FunctionType.Go, new List<string>() { pointNum.ToString(), xOffset.ToString(), yOffset.ToString(), zOffset.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { PointNum = pointNum, XOffset = xOffset, YOffset = yOffset, ZOffset = zOffset, SendData = oneFrame }, FunctionType.Go, null);
        }

        public void Jump(int pointNum, double xOffset = 0, double yOffset = 0, double zOffset = 0)
        {
            var oneFrame = FillOneFrame(FunctionType.Jump, new List<string>() { pointNum.ToString(), xOffset.ToString(), yOffset.ToString(), zOffset.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { PointNum = pointNum, ZOffset = zOffset, SendData = oneFrame }, FunctionType.Jump, null);
        }

        public void SetSpeed(int maxSpeedPercent)
        {
            var oneFrame = FillOneFrame(FunctionType.Speed, new List<string>() { maxSpeedPercent.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.Speed, null);
        }

        public void SetAccel(int maxAccelPercent, int maxDecelPercent)
        {
            var oneFrame = FillOneFrame(FunctionType.Accel, new List<string>() { maxAccelPercent.ToString(), maxDecelPercent.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.Accel, null);
        }

        public void PowerHigh()
        {
            var oneFrame = FillOneFrame(FunctionType.PowerHigh, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.PowerHigh, null);
        }

        public void PowerLow()
        {
            var oneFrame = FillOneFrame(FunctionType.PowerLow, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.PowerLow, null);
        }

        public void ResetCmd()
        {
            var oneFrame = FillOneFrame(FunctionType.Reset, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.Reset, null);
        }

        public void Reset()
        {
            ResetCmd();
            PowerLow();
            SetSpeed(_config.Speed);
            SetAccel(_config.Accel, _config.Accel);
            LimitTorque(_config.LimitTorque);
            Jump(0);
            PowerHigh();
        }

        public void Pause()
        {
            var oneFrame = FillOneFrame(FunctionType.Pause, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.Pause, null);
        }

        public void Resume()
        {
            var oneFrame = FillOneFrame(FunctionType.Resume, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.Resume, null);
        }

        public void MemOn(int io)
        {
            var oneFrame = FillOneFrame(FunctionType.MemOn, new List<string>() { io.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.MemOn, null);
        }

        public void MemOff(int io)
        {
            var oneFrame = FillOneFrame(FunctionType.MemOff, new List<string>() { io.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.MemOff, null);
        }

        public void ExcuteScript(string rScript)
        {
            var oneFrame = FillOneFrame(FunctionType.ExcuteScript, new List<string>() { rScript });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.ExcuteScript, null);
        }

        public int GetIO(int io)
        {
            var oneFrame = FillOneFrame(FunctionType.GetIO, new List<string>() { io.ToString() });
            var sendData = Encoding.ASCII.GetBytes(oneFrame);

            int state = 0;
            Action<string[]> _getIO = args =>
            {
                state = int.Parse(args[0]);
            };

            CommandOfCommon(sendData, new { SendData = oneFrame }, FunctionType.GetIO, _getIO);
            return state;
        }

        public void SetIO(int io, IOState state)
        {
            var oneFrame = FillOneFrame(FunctionType.SetIO, new List<string>() { io.ToString(), ((int)state).ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.SetIO, null);
        }

        public void BGo(double x, double y, double z, double u)
        {
            var oneFrame = FillOneFrame(FunctionType.BGo, new List<string>() { x.ToString(), y.ToString(), z.ToString(), u.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);
            CommandOfCommon(buffer, new { X = x, Y = y, Z = z, U = u, SendData = oneFrame }, FunctionType.BGo, null);
        }

        public Dictionary<string, double> Current()
        {
            var oneFrame = FillOneFrame(FunctionType.Current, new List<string>() { });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);

            Dictionary<string, double> pos = null;

            Action<string[]> _current = args =>
            {
                pos = new Dictionary<string, double> { { "X", double.Parse(args[0]) }, { "Y", double.Parse(args[1]) }, { "Z", double.Parse(args[2]) },
                    { "U", double.Parse(args[3]) }, { "V", double.Parse(args[4]) }, { "W", double.Parse(args[5]) } };
            };

            CommandOfCommon(buffer, new { SendData = oneFrame }, FunctionType.Current, _current);

            return pos;
        }

        public Dictionary<string, double> GetPos(int pointNum)
        {
            var oneFrame = FillOneFrame(FunctionType.GetPos, new List<string>() { pointNum.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);

            Dictionary<string, double> pos = null;

            Action<string[]> _current = args =>
            {
                pos = new Dictionary<string, double> { { "X", double.Parse(args[0]) }, { "Y", double.Parse(args[1]) }, { "Z", double.Parse(args[2]) },
                    { "U", double.Parse(args[3]) }, { "V", double.Parse(args[4]) }, { "W", double.Parse(args[5]) } };
            };

            CommandOfCommon(buffer, new { PointNum = pointNum, SendData = oneFrame }, FunctionType.GetPos, _current);

            return pos;
        }

        public Dictionary<string, double> GetPalletPos(int palletNo, int colrowNo)
        {
            var oneFrame = FillOneFrame(FunctionType.GetPalletPos, new List<string>() { palletNo.ToString(), colrowNo.ToString() });
            byte[] buffer = Encoding.ASCII.GetBytes(oneFrame);

            Dictionary<string, double> pos = null;

            Action<string[]> _current = args =>
            {
                pos = new Dictionary<string, double> { { "X", double.Parse(args[0]) }, { "Y", double.Parse(args[1]) }, { "Z", double.Parse(args[2]) },
                    { "U", double.Parse(args[3]) }, { "V", double.Parse(args[4]) }, { "W", double.Parse(args[5]) } };
            };

            CommandOfCommon(buffer, new { PalletNo = palletNo, ColRowNo = colrowNo, SendData = oneFrame }, FunctionType.GetPalletPos, _current);

            return pos;
        }

        /// <summary>
        /// 转移到托盘具体的行列号
        /// </summary>
        /// <param name="palletNo">托盘号</param>
        /// <param name="colRowNo">行列号</param>
        public void GoPalletNo(int palletNo, int row, int column, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0)
        {
            ExcuteScript($"go pallet({palletNo},{column},{row}) +X({xOffset}) +Y({yOffset}) +Z({zOffset})");
        }
        public void JumpPalletNo(int palletNo, int row, int column, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0)
        {
            ExcuteScript($"jump pallet({palletNo},{column},{row}) +X({xOffset}) +Y({yOffset}) +Z({zOffset})");
        }
        public void Move(int pointNum, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0)
        {
            ExcuteScript($"move p{pointNum} +X({xOffset}) +Y({yOffset}) + Z({zOffset})");
        }

        /// <summary>
        /// 设置所有轴的力矩（最大力矩百分比），力矩越小，碰撞失步需要的力度也越小
        /// </summary>
        /// <param name="percent"></param>
        public void LimitTorque(int percent)
        {
            ExcuteScript($"LimitTorque {percent}");
        }

        public void MoveSpeed(int speed)
        {
            ExcuteScript($"SpeedS {speed}");
        }

        public void MoveAccel(int accel)
        {
            ExcuteScript($"AccelS {accel}");
        }
        #endregion


        #region private method

        /// <summary>
        /// 返回错误响应类型
        /// </summary>
        /// <param name="revStr"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Exception ReceiveDataCheck(Tuple<string, string, string[]> revStr, FunctionType type)
        {
            if (revStr.Item1 == "[3]")
            {
                return new Exception($"Method: {type.ToString()}, response error code: {revStr.Item3[0]}, error message: {revStr.Item3[1]}");
            }
            return null;
        }

        /// <summary>
        /// 解析接收一帧数据，返回响应类型，响应方法，响应参数
        /// </summary>
        /// <param name="rcvData"></param>
        /// <returns></returns>
        private Tuple<string, string, string[]> ResolveRcvData(string rcvData)
        {
            string removeHeadTail = rcvData.Substring(cmdHead.Length, rcvData.Length - cmdHead.Length - cmdTail.Length);
            string resultType = removeHeadTail.Substring(0, 3);
            string[] results = removeHeadTail.Substring(3).Split(methodSeparator[0]);
            string method = results[0];
            string[] paramers = results[1].Split(paramsSeparator[0]);
            return new Tuple<string, string, string[]>(resultType, method, paramers);
        }

        /// <summary>
        /// 提取出公共部分，不一样的部分（body中间数据部分，responseID响应ID，action处理接收数据）
        /// </summary>
        /// <param name="body"></param>
        /// <param name="responseID"></param>
        /// <param name="action"></param>
        private void CommandOfCommon(byte[] sendBytes, object info, FunctionType type, Action<string[]> action)
        {
            bool compelete = false;
            Exception exception = null;

            EventHandler<string> _listen = (sender, argus) =>
            {
                var resolve = ResolveRcvData(argus);

                if (resolve.Item2 == type.ToString())
                {
                    exception = ReceiveDataCheck(resolve, type);
                    //处理接收数据
                    action?.Invoke(resolve.Item3);

                    compelete = true;
                };
            };

            Action _sendMsg = () =>
            {
                try
                {
                    _oneFrameArrive += _listen;
                    Send(sendBytes);
                    var result = (this as object).WaitForEnd(TimeSpan.FromSeconds(_config.RcvTimeOut), TimeSpan.FromMilliseconds(50),
                        () => compelete);

                    if (!result)
                    {
                        throw new Exception($"{type.ToString()}: receive data timeout(s)");
                    }
                    _oneFrameArrive -= _listen;

                    if (exception != null)
                        throw exception;
                }
                catch (Exception ex)
                {
                    _oneFrameArrive -= _listen;
                    throw;
                }
            };
            Execute(_sendMsg, info, type.ToString());
        }

        private void Execute(Action action, object info, string methodName)
        {
            lock (MyLock)
            {
                var begin = DateTime.UtcNow;
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    var errMsg = $"Encountered error, {ex.Message}";
                    throw new Exception($"{errMsg}", ex);
                }
            }
        }

        private string FillOneFrame(FunctionType type, List<string> paramers)
        {
            string oneFrame = cmdHead + "[1]" + type.ToString() + methodSeparator;
            for (int i = 0; i < paramers.Count; i++)
            {
                if (i < paramers.Count - 1)
                    oneFrame = oneFrame + paramers[i] + paramsSeparator;
                else
                    oneFrame = oneFrame + paramers[i];
            }

            oneFrame = oneFrame + cmdTail;
            return oneFrame;
        }

        /// <summary>
        /// 一个不断读取串口接收数据的线程，为保证数据的完整性，根据头帧以及数据长度，判断是否有完整数据
        /// </summary>
        private void RevData()
        {
            byte[] buffer;
            while (true)
            {
                if (_tokenSource.Token.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    buffer = new byte[1024];
                    if (_streamToServer.DataAvailable)
                        _streamToServer.Read(buffer, 0, buffer.Length);

                    string str = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty).Replace("\r\n", string.Empty);
                    _rcvDatas += str;

                    if (_rcvDatas.Contains(cmdTail))
                    {
                        var tailIndex = _rcvDatas.IndexOf(cmdTail);
                        string oneFrame = _rcvDatas.Substring(0, tailIndex + cmdTail.Length);
                        _rcvDatas = _rcvDatas.Substring(tailIndex + cmdTail.Length);

                        if (oneFrame.Contains(cmdHead))
                        {
                            var headIndex = oneFrame.IndexOf(cmdHead);
                            if (tailIndex > headIndex)
                            {
                                oneFrame = oneFrame.Substring(headIndex);
                                _oneFrameArrive?.Invoke(null, oneFrame);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sendBytes"></param>
        private void Send(byte[] sendBytes)
        {
            try
            {
                _streamToServer.Write(sendBytes, 0, sendBytes.Length);
            }
            catch (Exception ex)
            {
                throw new Exception($"Tongs send data error:{ex.Message}", ex);
            }
        }
        #endregion

    }

    /// <summary>
    /// 方法类型
    /// </summary>
    public enum FunctionType : int
    {
        Jump = 1,
        Reset = 2,
        MotorOn = 3,
        MotorOff = 4,
        PowerHigh = 5,
        PowerLow = 6,
        MemOn = 7,
        MemOff = 8,
        Speed = 9,
        Accel = 10,
        Pause = 11,
        Resume = 12,
        ExcuteScript = 13,
        GripperDrive = 14,
        GetIO = 15,
        SetIO = 16,
        BGo = 17,
        Current = 18,
        GetPos = 19,
        GetPalletPos = 20,
        Go = 21
    }

    /// <summary>
    /// IO点，In代表用于setIO，Out代表用于getIO
    /// </summary>
    public enum IONumber : int
    {
        In_0 = 0,
        In_1 = 1,
        In_2 = 2,
        In_Setup = 3,
        In_Drive = 4,
        In_Reset = 5,
        In_Svon = 6,
        Out_Seton = 11,
        Out_Inp = 12,
        Out_Svre = 13,
        Out_Alarm = 14,
        Out_Busy = 15,
    }

    public enum IOState
    {
        Off = 0,
        On = 1
    }
}
