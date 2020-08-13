using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class TaskPrepareContent
    {
        [JsonProperty("task_id")]
        public string TaskId { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("app")]
        public string App { get; set; }
        [JsonProperty("script")]
        public string Script { get; set; }
        [JsonProperty("layout")]
        public List<Layout> Layouts { get; set; }
        [JsonProperty("prep_time")]
        public DateTime PrepTime { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
    }
}
