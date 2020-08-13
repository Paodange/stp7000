namespace Mgi.Instrument.ALM.Device
{
    /// <summary>
    /// 移液器位置信息
    /// </summary>
    public class PipettePosition
    {
        /// <summary>
        ///  名称
        /// </summary>
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
}
