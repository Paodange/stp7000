namespace Mgi.Instrument.ALM.Device
{
    /// <summary>
    /// 拔盖器C轴松开状态
    /// </summary>
    public enum LidUncoverCReleaseLevel
    {
        /// <summary>
        /// 张开大一点  机械臂放下时 多松开1毫米
        /// </summary>
        ForLoosen,
        /// <summary>
        /// 张开小一点 
        /// </summary>
        ForGrasp
    }
}
