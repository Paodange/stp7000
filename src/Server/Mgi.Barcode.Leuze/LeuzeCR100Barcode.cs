using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mgi.Barcode.Leuze
{
    public class LeuzeCR100Barcode : ILeuzeBarcode
    {
        private readonly object syncLock = new object();
        /// <summary>
        /// 连续扫码超时时间(毫秒)
        /// </summary>
        /// 
        private const int OperationTimeOut = 10 * 1000;
        private const string CMD_END = "\r\n";
        private readonly SerialPort serialPort;
        private readonly Encoding serialPortEncoding = Encoding.ASCII;
        public LeuzeCR100Barcode(SerialPortConfig config)
        {
            serialPort = new SerialPort(config.PortName, config.BaudRate, config.Parity,
                 config.DataBits, config.StopBits)
            {
                ReadTimeout = OperationTimeOut,
                WriteTimeout = OperationTimeOut,
                Encoding = serialPortEncoding,
                NewLine = CMD_END
            };
        }

        public string GetVersion()
        {
            lock (syncLock)
            {
                return SendAndReceive(BuildCommandData("V"));
            }
        }
        public string SingleTrigger()
        {
            lock (syncLock)
            {
                Send(BuildCommandData("+"));
                try
                {
                    return Receive();
                }
                catch (TimeoutException)
                {
                    // 未扫到条码时，扫码枪不响应任何数据   
                    // 超时时间到未扫到条码  当作扫不到条码返回空处理
                    return string.Empty;
                }
            }
        }
        public string ConsequentTrigger()
        {
            lock (syncLock)
            {
                SendAndReceive(BuildCommandData("PT03203401"));  //开始连续扫码
                var code = string.Empty;
                try
                {
                    code = Receive();
                }
                catch (TimeoutException)
                {
                    // 未扫到条码时，扫码枪不响应任何数据   
                    // 超时时间到未扫到条码  当作扫不到条码返回空处理
                }
                SendAndReceive(BuildCommandData("PT03203400"));  // 关闭连续扫码
                Thread.Sleep(300);
                return code;
            }
        }

        private Task workingTask;
        private int sl = 0;
        private string barcode = string.Empty;
        public void BeginConsequentTrigger()
        {
            if (Interlocked.CompareExchange(ref sl, 1, 0) == 1) return;
            SendAndReceive(BuildCommandData("PT03203401"));  //开始连续扫码
            barcode = string.Empty;
            workingTask = Task.Run(() =>
             {
                 try
                 {
                     barcode = Receive();
                 }
                 catch (TimeoutException)
                 {
                     SendAndReceive(BuildCommandData("PT03203400"));  // 关闭连续扫码
                     // 未扫到条码时，扫码枪不响应任何数据   
                     // 超时时间到未扫到条码  当作扫不到条码返回空处理
                 }
                 catch (Exception)
                 {

                 }
             });
        }

        public string EndConsequentTrigger()
        {
            if (Interlocked.CompareExchange(ref sl, 0, 1) == 0)
            {
                return string.Empty;
            }
            SendAndReceive(BuildCommandData("PT03203400"));  // 关闭连续扫码
            Thread.Sleep(100);
            var code = barcode;
            barcode = string.Empty;
            return code;
        }
        private string SendAndReceive(byte[] data)
        {
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();
            serialPort.Write(data, 0, data.Length);
            return Receive();
        }
        private void Send(byte[] data)
        {
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();
            serialPort.Write(data, 0, data.Length);
        }
        private string Receive()
        {
            var resp = serialPort.ReadLine();
            if (!string.IsNullOrWhiteSpace(resp))
            {
                resp = resp.Remove(0, 1);
            }
            return resp;
        }
        private byte[] BuildCommandData(string body)
        {
            return BuildCommandData(serialPortEncoding.GetBytes(body));
        }
        private byte[] BuildCommandData(byte[] body)
        {
            byte[] data = new byte[3 + body.Length];
            data[0] = 0x02;
            Buffer.BlockCopy(body, 0, data, 1, body.Length);
            data[data.Length - 2] = 0x0D;
            data[data.Length - 1] = 0x0A;
            return data;
        }
        public void Open()
        {
            if (serialPort.IsOpen) return;
            try
            {
                serialPort.Open();
                SendAndReceive(BuildCommandData("PT03203400"));
            }
            catch (Exception ex)
            {
                throw new Exception($"Open serial port error:{ex.Message}", ex);
            }
        }
        public void Close()
        {
            if (!serialPort.IsOpen) return;
            try
            {
                serialPort.Close();
            }
            catch (IOException)
            {
                //
            }
        }


    }
}
