namespace Mgi.Instrument.ALM
{
    /// <summary>
    /// 滑块试管位  枚举值 有特定意义 请不要修改
    /// </summary>
    public enum SliderSamplePos : byte
    {
        POS1 = 0,
        POS2 = 1
    }


    /// <summary>
    ///  滑块试管位状态 枚举值 有特定意义 请不要修改
    /// </summary>
    public enum SliderSamplePosState
    {
        // 枚举值 有特定意义 请不要修改
        Occupied = 0,
        Empty = 1,
    }
}
