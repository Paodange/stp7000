using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class CompleteOutput
    {
        [JsonProperty("source_pos")]
        public string SourcePos { get; set; }
        [JsonProperty("source_well")]
        public string SourceWell { get; set; }
        [JsonProperty("target_pos")]
        public string TargetPos { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
