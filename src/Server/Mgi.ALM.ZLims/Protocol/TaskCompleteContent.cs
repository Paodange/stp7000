using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class TaskCompleteContent
    {
        [JsonProperty("task_id")]
        public string TaskId { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("app")]
        public string App { get; set; }
        [JsonProperty("script")]
        public string Script { get; set; }
        [JsonProperty("complete_status")]
        public bool CompleteStatus { get; set; }
        [JsonProperty("outputs")]
        public List<CompleteOutput> Outputs { get; set; }
        [JsonProperty("complete_time")]
        public DateTime CompleteTime { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
    }
}
