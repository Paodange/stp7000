
using Mgi.Robot.Cantroller.Axis;
using System;
using System.Collections.Generic;

namespace Mgi.Robot.Cantroller
{
    public class RobotConfig
    {
        public string HandleName { get; set; }       


        /// <summary>
        /// 设备类型号
        /// </summary>
        public uint DeviceType { get; set; }

        /// <summary>
        /// 设备索引号
        /// </summary>
        public uint DeviceIndex { get; set; }

        /// <summary>
        /// CAN 口索引
        /// </summary>
        public uint CanIndex { get; set; }

        /// <summary>
        /// CAN卡IO时间
        /// </summary>
        public TimeSpan CanTimeout { get; set; }

        /// <summary>
        /// 用来区分是PCB
        /// </summary>
        public byte FrameId { get; set; }

        public bool Simulated { get; set; }

        /// <summary>
        /// 轴配置信息
        /// </summary>
        public List<AxisConfig> AxisConfig { get; set; }
    }
    
}
