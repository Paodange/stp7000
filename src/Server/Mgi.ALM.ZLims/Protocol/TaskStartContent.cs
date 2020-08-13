using System;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class TaskStartContent
    {
        [JsonProperty("task_id")]
        public string TaskId { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("app")]
        public string App { get; set; }
        [JsonProperty("script")]
        public string Script { get; set; }
        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
    }
}
