using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Barcode.Leuze
{
    /// <summary>
    ///  Config
    /// </summary>
    public class SerialPortConfig
    {
        //string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)

        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        /// <summary>
        /// 扫码枪类型   1. CR100   2. DCR55 3. CONEX DM50
        /// </summary>
        public int Type { get; set; } = 1;
        public bool Simulated { get; set; } = false;
    }
}
