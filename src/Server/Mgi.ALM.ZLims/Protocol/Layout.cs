using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class Layout
    {
        [JsonProperty("position")]
        public string Position { get; set; }
        [JsonProperty("barcode")]
        public string Barcode { get; set; }
        [JsonProperty("well")]
        public string Well { get; set; }
        [JsonProperty("prompt")]
        public string Prompt { get; set; }
    }
}
