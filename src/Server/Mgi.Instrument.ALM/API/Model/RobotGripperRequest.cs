using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mgi.Instrument.ALM.API.Model
{
    public class RobotGripperRequest
    {
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
    }
}
