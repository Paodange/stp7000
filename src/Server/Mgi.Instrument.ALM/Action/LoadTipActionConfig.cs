namespace Mgi.Instrument.ALM.Action
{
    /// <summary>
    /// 扎吸头动作配置
    /// </summary>
    public class LoadTipActionConfig
    {

        /// <summary>
        /// (毫米) Z轴第一段下降的 相对底部学习位置的偏移   (第一段下降距离为底部Z绝对脉冲-此偏移) 
        /// </summary>
        public double ZFirstMoveOffset { get; set; }

        /// <summary>
        ///  (毫米) 扎吸头第二段下降相对于底部学习位置的偏移  (第二段下降距离为底部Z绝对脉冲+此偏移)
        /// </summary>
        public double LoadTipOffset { get; set; }
        /// <summary>
        /// 扎吸头速度
        /// </summary>
        public int LoadTipSpeed { get; set; }

        /// <summary>
        /// 扎吸头加速度
        /// </summary>
        public int LoadTipAccel { get; set; }
        /// <summary>
        /// 最小丢步数
        /// </summary>
        public int MinAccuracy { get; set; } = 0;
        /// <summary>
        /// 最大丢步数
        /// </summary>
        public int MaxAccuracy { get; set; } = 6000;
    }
}
