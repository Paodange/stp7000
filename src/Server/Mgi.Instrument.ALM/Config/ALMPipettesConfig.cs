using Mgi.Instrument.ALM.Action;
using Mgi.Instrument.ALM.Device;
using Mgi.Robot.Cantroller;
using System.Collections.Generic;

namespace Mgi.Instrument.ALM.Config
{
    public class ALMPipettesConfig
    {
        /// <summary>
        ///  Y1,Y2 两个轴的距离补偿，以Y2(A通道)为基准  保证Y1距离Y1为9mm的  B偿值 负数表示向Y2远离方向，正数向Y2靠近方向 
        /// </summary>
        public double YCompensate { get; set; }
        /// <summary>
        ///  Z1,Z2两个轴的距离补偿，以Z2(B通道)为基准  保证Z1 Z2垂直方向一致  Y1的补偿值 
        /// </summary>
        public double ZCompensate { get; set; }
        public List<PipettePosition> Positions { get; set; }

        /// <summary>
        /// 扎吸头配置
        /// </summary>
        public LoadTipActionConfig LoadTipConfig { get; set; }
        /// <summary>
        /// 退吸头配置
        /// </summary>
        public UnloadTipActionConfig UnloadTipConfig { get; set; } = new UnloadTipActionConfig();

        /// <summary>
        /// 吸液配置
        /// </summary>
        public AspirateActionConfig AspirateConfig { get; set; }
        /// <summary>
        /// 排液配置
        /// </summary>
        public UnAspirateActionConfig UnAspirateConfig { get; set; }
        /// <summary>
        /// 吸液补偿参数
        /// </summary>
        public List<CompensateConfig> CompensateConfigs { get; set; }
        /// <summary>
        /// 电机配置
        /// </summary>
        public List<RobotConfig> RobotConfigs { get; set; }
    }
}
