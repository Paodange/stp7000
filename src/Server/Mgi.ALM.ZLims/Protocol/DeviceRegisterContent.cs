using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mgi.ALM.ZLims.Protocol
{
    public class DeviceRegisterContent
    {
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        [JsonProperty("device_type")]
        public string DeviceType { get; set; }

        [JsonProperty("device_desc")]
        public string DeviceDesc { get; set; }

        [JsonProperty("components")]
        public List<DeviceComponent> Components { get; set; }
    }

    public class DeviceComponent
    {
        [JsonProperty("component_id")]
        public string ComponentId { get; set; }

        [JsonProperty("component_type")]
        public string ComponentType { get; set; }

        [JsonProperty("component_desc")]
        public string ComponentDesc { get; set; }
    }
}
