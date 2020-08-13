using Mgi.Instrument.ALM.Device;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mgi.Instrument.ALM.API.Model
{
    public class PipetteRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PipettePositions Position { get; set; }
    }
    public class PipetteChannelRequest : PipetteRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PipetteChannels Channel { get; set; }
    }

    public class UnloadTipRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PipetteChannels Channel { get; set; }
    }

    public class PipetteChannelAspirateRequest : PipetteChannelRequest
    {
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
        public double Volume { get; set; }
    }

    public class PipetteAspirateRequest : PipetteRequest
    {
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
        public double Volume { get; set; }
    }
    public class PipetteMoveRequest : PipetteRequest
    {
        public int Row { get; set; } = 1;
        public int Column { get; set; } = 1;
    }
}
