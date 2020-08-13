using System;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class DeviceLogContent
    {
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("component_id")]
        public string ComponentId { get; set; }
        [JsonProperty("task_id")]
        public string TaskId { get; set; }
        [JsonProperty("app")]
        public string App { get; set; }
        [JsonProperty("script")]
        public string Script { get; set; }
        [JsonProperty("log_code")]
        public string LogCode { get; set; }
        [JsonProperty("log_type")]
        public string LogType { get; set; }
        [JsonProperty("log_level")]
        public string LogLevel { get; set; }
        [JsonProperty("log_desc")]
        public string LogDesc { get; set; }
        [JsonProperty("log_time")]
        public DateTime LogTime { get; set; }
        [JsonProperty("handling")]
        public string Handing { get; set; }
    }
}


//device_id
//component_id
//task_id
//app
//script
//log_code
//log_type
//log_level
//log_desc
//log_time
//handling
