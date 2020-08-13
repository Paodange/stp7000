namespace Mgi.Instrument.ALM.Action
{
    /// <summary>
    /// 排液动作配置
    /// </summary>
    public class UnAspirateActionConfig
    {
        /// <summary>
        /// 排液高度  Z轴下降高度为  学习位置-此高度
        /// </summary>
        public double UnAspirateHeight { get; set; }
        /// <summary>
        /// Z轴上抬第一段速度
        /// </summary>
        public int ZFirstUpSpeed { get; set; }
        /// <summary>
        ///  Z轴上抬第一段加速度
        /// </summary>
        public int ZFirstUpAccel { get; set; }

        /// <summary>
        /// 排液 速度  P轴
        /// </summary>
        public int UnAspirateSpeed { get; set; }
        /// <summary>
        /// 排液加速度 P轴
        /// </summary>
        public int UnAspirateAccel { get; set; }

        /// <summary>
        /// 排液后等待时间(毫秒)
        /// </summary>
        public int UnAspirateDelay { get; set; } = 500;
        /// <summary>
        /// 第一次上抬 偏移(毫米)
        /// </summary>
        public double ZFirstUpOffset { get; set; }
    }
}
