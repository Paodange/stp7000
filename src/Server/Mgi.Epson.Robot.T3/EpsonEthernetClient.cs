using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mgi.Epson.Robot.T3;
using Mgi.Epson.Robot.T3.Extentions;

namespace Mgi.Epson.Robot
{
    public class EpsonEthernetClient
    {

        private const string SendHead = "$";
        private const string ResponseHead = "#";
        private const string ErrorHead = "!";
        private const string cmdTail = "\r\n";

        private enum FunctionType
        {
            Login = 0,
            Start = 1,
            Stop = 2,
            Pause = 3,
            Continue = 4,
            SetIO = 5,
            GetIO = 6,
            Reset = 7
        }
        public enum IOState
        {
            Off = 0,
            On = 1
        }

        public TcpClient TcpClient { get; private set; }
        private NetworkStream _streamToServer;

        private Task _revDataTask;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private event EventHandler<string> _oneFrameArrive;
        private string _rcvDatas;


        private EpsonRobotConfig _config;
        private readonly object MyLock = new object();

        public EpsonEthernetClient(EpsonRobotConfig config)
        {
            _config = config;
        }


        #region public method
        public void Open()
        {
            TcpClient = new TcpClient();
            try
            {

                TcpClient.Connect(_config.EpsonIP, _config.EthernetPort);
                _streamToServer = TcpClient.GetStream();

                _tokenSource = new CancellationTokenSource();
                _revDataTask = new Task(() => RevData(), _tokenSource.Token);
                _revDataTask.Start();

            }
            catch (Exception ex)
            {
                throw new Exception($"Connect fail:{ex.Message}", ex);
            }
        }

        public void Close()
        {
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

            if (TcpClient != null)
            {
                try
                {
                    TcpClient.Close();
                    TcpClient.Dispose();
                    _revDataTask = null;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Tcp close fail:{ex.Message}", ex);
                }
            }
        }

        public void Login()
        {
            var oneFrame = FillOneFrame(FunctionType.Login, new List<string>() { "0" });
            var sendData = Encoding.ASCII.GetBytes(oneFrame);

            CommandOfCommon(sendData, new { SendData = oneFrame }, FunctionType.Login, null);
        }

        public void StartMain()
        {
            var oneFrame = FillOneFrame(FunctionType.Start, new List<string>() { "0" });
            var sendData = Encoding.ASCII.GetBytes(oneFrame);

            CommandOfCommon(sendData, new { SendData = oneFrame }, FunctionType.Start, null);
        }

        public void Stop()
        {
        }

        public void Pause()
        {
        }

        public void Continue()
        {
        }

        public void Reset()
        {
            var oneFrame = FillOneFrame(FunctionType.Reset, new List<string>() { });
            var sendData = Encoding.ASCII.GetBytes(oneFrame);

            CommandOfCommon(sendData, new { SendData = oneFrame }, FunctionType.Reset, null);
        }

        public int GetIO(int io)
        {
            var oneFrame = FillOneFrame(FunctionType.GetIO, new List<string>() { io.ToString() });
            var sendData = Encoding.ASCII.GetBytes(oneFrame);

            int state = 0;
            Action<string[]> _getIO = args =>
            {
                state = int.Parse(args[1]);
            };

            CommandOfCommon(sendData, new { SendData = oneFrame }, FunctionType.GetIO, _getIO);

            return state;
        }

        public void SetIO(int io, IOState state)
        {
            var oneFrame = FillOneFrame(FunctionType.SetIO, new List<string>() { io.ToString(), ((int)state).ToString() });
            var sendData = Encoding.ASCII.GetBytes(oneFrame);

            CommandOfCommon(sendData, new { SendData = oneFrame }, FunctionType.SetIO, null);
        }
        #endregion

        #region private method
        private string FillOneFrame(FunctionType type, List<string> paramers)
        {
            string oneFrame = SendHead + type.ToString() + ",";
            paramers.ForEach(p => oneFrame = oneFrame + p + ",");
            oneFrame = oneFrame.Remove(oneFrame.Length - 1, 1);

            oneFrame = oneFrame + cmdTail;
            return oneFrame;
        }

        /// <summary>
        /// 解析接收一帧数据，返回响应类型，响应方法，响应参数
        /// </summary>
        /// <param name="rcvData"></param>
        /// <returns></returns>
        private string[] ResolveRcvData(string rcvData)
        {
            rcvData = rcvData.Remove(0, 1).Replace("\r\n", "");
            var errorCode = rcvData.Split(',');
            return errorCode;
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
                if (argus.Contains(ResponseHead))
                {
                    var resolve = ResolveRcvData(argus);
                    if (resolve[0] == type.ToString())
                    {
                        //处理接收数据
                        action?.Invoke(resolve);
                    };
                }
                else
                {
                    var errorCode = argus.Split(',')[1];
                    exception = new Exception($"{type.ToString()}: errorCode = {errorCode}");
                }
                compelete = true;

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
                    throw;
                }
            }
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
                    //_logger.Debug($"TempName:{tempName}. Temperature receive data task end");
                    //_tokenSource.Token.ThrowIfCancellationRequested();
                }
                Thread.Sleep(10);
                try
                {
                    buffer = new byte[1024];
                    if (_streamToServer.DataAvailable)
                        _streamToServer.Read(buffer, 0, buffer.Length);

                    string str = Encoding.ASCII.GetString(buffer).Replace("\0", string.Empty);
                    _rcvDatas += str;

                    if (_rcvDatas.Contains(cmdTail))
                    {
                        var tailIndex = _rcvDatas.IndexOf(cmdTail);
                        string oneFrame = _rcvDatas.Substring(0, tailIndex + cmdTail.Length);
                        _rcvDatas = _rcvDatas.Substring(tailIndex + cmdTail.Length);

                        if (oneFrame.Contains(ResponseHead) || oneFrame.Contains(ErrorHead))
                        {
                            var headIndex = oneFrame.IndexOf(ResponseHead);
                            if (headIndex < 0)
                                headIndex = oneFrame.IndexOf(ErrorHead);
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
                    Thread.Sleep(30);
                }
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
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                throw new Exception($"Tongs send data error:{ex.Message}", ex);
            }
        }
        #endregion
    }
}
