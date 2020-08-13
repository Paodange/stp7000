namespace Mgi.Instrument.ALM.Config
{

    /// <summary>
    /// 物料配置
    /// </summary>
    internal class MaterialConfig
    {
        public string Name { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        /// <summary>
        /// 行间距 (毫米)
        /// </summary>
        public double RowSpan { get; set; }

        /// <summary>
        /// 列间距 (毫米)
        /// </summary>
        public double ColumnSpan { get; set; }
    }
}
