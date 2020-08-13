namespace Mgi.Instrument.ALM.Action
{
    /// <summary>
    /// 退吸头动作配置
    /// </summary>
    public class UnloadTipActionConfig
    {
        /// <summary>
        /// 退吸头X相对学习位置应往前走的距离（毫米）
        /// </summary>
        public double UnloadTipXOffset { get; set; } = 10;
        /// <summary>
        /// 退吸头时 Z轴推掉吸头应上抬的距离 （毫米）
        /// </summary>
        public double UnloadTipZUpDistance { get; set; } = 10;
        /// <summary>
        /// 退吸头时 Z轴推掉吸头的电机速度
        /// </summary>
        public int UnloadTipZUpSpeed { get; set; } = 43200;
        /// <summary>
        /// 退吸头时 Z轴推掉吸头的电机加速度
        /// </summary>
        public int UnloadTipZUpAccel { get; set; } = 1200000;
    }
}
