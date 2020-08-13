using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mgi.Instrument.ALM.API.Model
{
    public class RobotRequest
    {
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RobotLocation Location { get; set; }

        public int Row { get; set; } = 1;
        public int Column { get; set; } = 1;
    }
}
