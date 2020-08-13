namespace Mgi.Instrument.ALM.Action
{
    /// <summary>
    /// 吸液动作配置
    /// </summary>
    public class AspirateActionConfig
    {
        /// <summary>
        /// Z轴下降第二段速度
        /// </summary>
        public int ZSecondSpeed { get; set; }
        /// <summary>
        ///  Z轴下降第二段加速度
        /// </summary>
        public int ZSecondAccel { get; set; }

        /// <summary>
        /// 吸液 速度  P轴
        /// </summary>
        public int AspirateSpeed { get; set; }
        /// <summary>
        /// 吸液加速度 P轴
        /// </summary>
        public int AspirateAccel { get; set; }

        /// <summary>
        /// 吸液后等待时间(毫秒)
        /// </summary>
        public int AspirateDelay { get; set; } = 500;
        /// <summary>
        /// 前吸体积（ul）
        /// </summary>
        public int FrontAspirateVolume { get; set; }

        /// <summary>
        /// 后吸体积
        /// </summary>
        public int BackAspirateVolume { get; set; }
    }
}
