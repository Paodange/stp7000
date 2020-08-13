namespace Mgi.Instrument.ALM.Action
{
    /// <summary>
    /// 盖回盖子动作配置
    /// </summary>
    public class CoverActionConfig
    {
        /// <summary>
        /// 盖回盖子相对于开盖子Z轴位置  多下降的毫米数
        /// </summary>
        public double CoverDownOffset { get; set; }

        /// <summary>
        /// 第二次旋转保持电流
        /// </summary>
        public int CoverSecondCurrent { get; set; } = 10;
    }
}
