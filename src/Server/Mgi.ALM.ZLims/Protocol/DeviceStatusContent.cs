using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.ALM.ZLims.Protocol
{
    public class DeviceStatusContent
    {
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_time")]
        public DateTime StatusTime { get; set; }

        [JsonProperty("components")]
        public List<ComponentStatus> Components { get; set; }
    }

    public class ComponentStatus
    {
        [JsonProperty("component_id")]
        public string ComponentId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
