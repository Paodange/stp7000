using Mgi.Barcode.Leuze;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Action;
using Mgi.Instrument.ALM.Device;
using Mgi.Robot.Cantroller;
using System.Collections.Generic;

namespace Mgi.Instrument.ALM.Config
{
    public class ALMLidUnCoverConfig
    {
        /// <summary>
        /// 扫码枪配置
        /// </summary>
        public Dictionary<string, SerialPortConfig> BarcodeConfigs { get; set; }
        /// <summary>
        /// 夹爪配置
        /// </summary>
        public Dictionary<string, ModbusGripperConfig> GripperConfigs { get; set; }
        /// <summary>
        /// 电机配置
        /// </summary>
        public List<RobotConfig> RobotConfigs { get; set; }

        /// <summary>
        /// 滑块位置
        /// </summary>
        public List<SliderPosition> SliderPositions { get; set; }

        /// <summary>
        ///  拔盖时 Z下降的脉冲数
        /// </summary>
        public Dictionary<LidUncoverUnits, int> LidBottomOfZs { get; set; }

        /// <summary>
        ///  Z轴安全高度  开完盖子回去的高度绝对脉冲
        /// </summary>
        public Dictionary<LidUncoverUnits, int> ZSafePositions { get; set; }
        /// <summary>
        ///  E轴基准位置 (脉冲)
        /// </summary>
        public Dictionary<LidUncoverUnits, int> EBasePositions { get; set; } = new Dictionary<LidUncoverUnits, int>()
        {
            { LidUncoverUnits.A,18139 },
            { LidUncoverUnits.B,18139 },
        };

        /// <summary>
        /// C轴夹紧时相对试管学习位置的偏移
        /// </summary>
        public double CTightenOffset { get; set; }
        /// <summary>
        /// C轴松开时相对试管学习位置的偏移  
        /// </summary>
        public double CReleaseOffset { get; set; }
        /// <summary>
        /// 机械臂放下时 相较于抓取 需要多松开的距离(毫米)
        /// </summary>
        public double CReleaseForLoosenOffset { get; set; } = 1;
        /// <summary>
        /// 盖盖子配置
        /// </summary>
        public CoverActionConfig CoverConfig { get; set; }

        /// <summary>
        /// 开盖子配置
        /// </summary>
        public UncoverActionConfig UncoverConfig { get; set; }

        /// <summary>
        /// 是否启用扫码功能  false  不启用   true 启用
        /// </summary>
        public bool ScannerEnable { get; set; } = true;

        /// <summary>
        /// 流程运行时 是否开启传感器检查
        /// </summary>
        public bool SliderSensorEnable { get; set; } = true;
    }
}
