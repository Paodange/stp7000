namespace Mgi.Instrument.ALM.Config
{
    /// <summary>
    /// 补偿参数配置  包括下限  不包括上限
    /// </summary>
    public class CompensateConfig
    {
        /// <summary>
        /// 体积下限
        /// </summary>
        public int MinVolume { get; set; }
        /// <summary>
        /// 体积上限
        /// </summary>
        public int MaxVolume { get; set; }
        /// <summary>
        /// 补偿系数a
        /// </summary>
        public double CompensateA { get; set; }
        /// <summary>
        ///  补偿系数b
        /// </summary>
        public double CompensateB { get; set; }
    }
}
