using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mgi.Barcode.Leuze
{
    public class ConexBarcode : ILeuzeBarcode
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
        SerialPortConfig Config { get; }
        public ConexBarcode(SerialPortConfig config)
        {
            Config = config;
            serialPort = new SerialPort(config.PortName, config.BaudRate, config.Parity,
            config.DataBits, config.StopBits)
            {
                ReadTimeout = OperationTimeOut,
                WriteTimeout = OperationTimeOut,
                DtrEnable = true,
                NewLine = CMD_END
            };

        }

        public void Open()
        {
            if (serialPort.IsOpen) return;
            try
            {
                serialPort.Open();
                SetResponseType(ResponseType.Silent);
                SetTriggerType(TriggerType.Continuous);
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

        public string GetVersion()
        {
            return SendAndReceive("||>GET DEVICE.FIRMWARE-VER\r\n");
        }

        public string SingleTrigger()
        {
            lock (syncLock)
            {
                try
                {
                    Send("+\r\n");
                    return Receive();
                }
                catch (TimeoutException)
                {
                    Send("-\r\n");  //关闭扫码
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
                try
                {
                    Send("+\r\n");
                    return Receive();
                }
                catch (TimeoutException)
                {
                    Send("-\r\n");  //关闭扫码
                    // 未扫到条码时，扫码枪不响应任何数据   
                    // 超时时间到未扫到条码  当作扫不到条码返回空处理
                    return string.Empty;
                }
            }
        }
        private int sl = 0;
        private string barcode = string.Empty;
        public void BeginConsequentTrigger()
        {
            if (Interlocked.CompareExchange(ref sl, 1, 0) == 1) return;
            Send("+\r\n");  //开始连续扫码
            barcode = string.Empty;
            Task.Run(() =>
            {
                try
                {
                    barcode = Receive();
                }
                catch (TimeoutException)
                {
                    Send("-\r\n");  //关闭扫码
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
            Send("-\r\n");  //关闭连续扫码
            Thread.Sleep(100);
            var code = barcode;
            barcode = string.Empty;
            return code;
        }
        private void Send(string cmd)
        {
            serialPort.WriteLine(cmd);
        }
        private string SendAndReceive(string cmd)
        {
            var data = serialPortEncoding.GetBytes(cmd);
            serialPort.Write(data, 0, data.Length);
            Thread.Sleep(100);
            return Receive();
        }
        private string Receive()
        {
            var resp = serialPort.ReadTo(CMD_END);
            //if (!string.IsNullOrWhiteSpace(resp))
            //{
            //    resp = resp.Remove(0, 1);
            //}
            return resp;
        }
        private void SetResponseType(ResponseType responseType)
        {
            Send($"||>COM.DMCC-RESPONSE {(int)responseType}\r\n");
        }
        private void SetTriggerType(TriggerType triggerType)
        {
            Send($"||>SET TRIGGER.TYPE {(int)triggerType}\r\n");
        }
    }

    public enum TriggerType
    {
        //0: Single (external)
        //1: Presentation (internal)
        //2: Manual(button)
        //3: Burst(external)
        //4: Self(internal)
        //5: Continuous(external)
        Single = 0,
        Presentation = 1,
        Manual,
        Burst,
        Self,
        Continuous
    }
    public enum ResponseType
    {
        Silent,
        Extended
    }
}
