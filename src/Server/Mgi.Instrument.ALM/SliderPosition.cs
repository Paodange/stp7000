using Mgi.Instrument.ALM.Device;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mgi.Instrument.ALM
{
    /// <summary>
    /// 滑块位置信息
    /// </summary>
    public class SliderPosition
    {
        /// <summary>
        /// 工作单元
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LidUncoverUnits LidUncover { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SliderPositionEnum Position { get; set; }
        /// <summary>
        /// 绝对脉冲数
        /// </summary>
        public int AbsPulse { get; set; }
    }
}
