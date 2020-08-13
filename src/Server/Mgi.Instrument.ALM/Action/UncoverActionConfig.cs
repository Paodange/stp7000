namespace Mgi.Instrument.ALM.Action
{
    /// <summary>
    /// 拧开盖子动作配置
    /// </summary>
    public class UncoverActionConfig
    {
        /// <summary>
        /// 开盖第二段Z轴上提速度
        /// </summary>
        public int UncoverUpSecondSpeed { get; set; } = 2000;
        /// <summary>
        /// 开盖第二段Z轴上提加速度
        /// </summary>
        public int UncoverUpSecondAccel { get; set; } = 600;
        /// <summary>
        /// 扫码旋转圈数
        /// </summary>
        public double ScanCycles { get; set; }

        /// <summary>
        /// 扫码速度
        /// </summary>
        public int ScanSpeed { get; set; }
        /// <summary>
        /// 扫码加速度
        /// </summary>
        public int ScanAccel { get; set; }
    }
}
