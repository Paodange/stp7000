using Mgi.Instrument.ALM.Axis;
using Mgi.Robot.Cantroller;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mgi.Instrument.ALM.Device
{
    public class PipetteChannel
    {
        /// <summary>
        /// 通道枚举
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PipetteChannels Channel { get; set; }
        /// <summary>
        /// X轴  多个通道共用
        /// </summary>
        public IALMAxis X { get; set; }
        /// <summary>
        /// Y轴  每个通道独立
        /// </summary>
        public IALMAxis Y { get; set; }
        /// <summary>
        /// Z轴  每个通道独立
        /// </summary>
        public IALMAxis Z { get; set; }
        /// <summary>
        /// P轴  每个通道独立
        /// </summary>
        public IALMAxis P { get; set; }

        public void HomeAll()
        {

        }
    }
}
