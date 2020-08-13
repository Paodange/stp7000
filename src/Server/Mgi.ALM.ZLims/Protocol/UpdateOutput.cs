using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public class UpdateOutput
    {
        [JsonProperty("source_pos")]
        public string SourcePos { get; set; }
        [JsonProperty("source_well")]
        public string SourceWell { get; set; }
        [JsonProperty("source_barcode")]
        public string SourceBarcode { get; set; }
        [JsonProperty("target_pos")]
        public string TargetPos { get; set; }
        [JsonProperty("target_well")]
        public string TargetWell { get; set; }
        [JsonProperty("target_barcode")]
        public string TargetBarcode { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }

    }
}
