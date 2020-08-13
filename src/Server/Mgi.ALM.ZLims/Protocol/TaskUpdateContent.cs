using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class TaskUpdateContent
    {
        [JsonProperty("task_id")]
        public string TaskId { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("app")]
        public string App { get; set; }
        [JsonProperty("script")]
        public string Script { get; set; }
        [JsonProperty("outputs")]
        public List<UpdateOutput> Outputs { get; set; }
        [JsonProperty("update_time")]
        public DateTime UpdateTime { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
    }
}
