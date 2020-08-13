using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Robot.Cantroller.Axis
{
    class AxisParameter
    {
        /// <summary>
        /// 为此Can指定的名字
        /// </summary>
        public string Name { get; set; }

        ///// <summary>
        ///// 设备类型号
        ///// </summary>
        //public uint DeviceType { get; set; }

        ///// <summary>
        ///// 设备索引号
        ///// </summary>
        //public uint DeviceIndex { get; set; }

        ///// <summary>
        ///// CAN 口索引
        ///// </summary>
        //public uint CanIndex { get; set; }

        /// <summary>
        /// 用来区分是PCB
        /// </summary>
        public byte FrameId { get; set; } 
        
        /// <summary>
        /// 电机号
        /// </summary>
        public byte Moto { get; set; }

        /// <summary>
        /// 等待读取的最长时间 ms
        /// </summary>
        public int WaitTimeout { get; set; }


    }
}
