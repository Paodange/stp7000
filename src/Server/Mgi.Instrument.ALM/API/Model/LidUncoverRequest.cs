using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mgi.Instrument.ALM.Device;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mgi.Instrument.ALM.API.Model
{
    public class LidUncoverRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LidUncoverUnits LidUncover { get; set; }
    }

    public class LidUncoverMoveRequest : LidUncoverRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public SliderPositionEnum SliderPosition { get; set; }
    }

    public class LidCoverConverRequest : LidUncoverRequest
    {
        /// <summary>
        /// 试管ID
        /// </summary>
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
    }
    public class LidUncoverAndScanRequest : LidUncoverRequest
    {
        /// <summary>
        /// 试管ID
        /// </summary>
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public LidUncoverGripper Gripper { get; set; }
    }


    public class GripperRequest : LidUncoverRequest
    {
        /// <summary>
        /// 夹爪名称 A 或者 B
        /// </summary>
        public string Gripper { get; set; }
    }
    public class TightenRequest : GripperRequest
    {
        public byte GripForce { get; set; }
        public ushort TeachPosition { get; set; }
        public byte PositionTolerance { get; set; }
        public bool ThrowIfEmptyGrasp { get; set; }
    }

    public class ScannerRequest : LidUncoverRequest
    {
        /// <summary>
        /// 扫码枪名称 A 或者 B
        /// </summary>
        public string Scanner { get; set; }
    }

    public class AssertSamplePosStateRequest : LidUncoverRequest
    {
        /// <summary>
        /// 试管位置
        /// </summary>
        public SliderSamplePos Pos { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public SliderSamplePosState State { get; set; }
    }

    public class LidUncoverTightenCRequest: LidUncoverRequest
    {
        /// <summary>
        /// 试管ID
        /// </summary>
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
    }
    public class LidUncoverReleaseCRequest : LidUncoverRequest
    {
        /// <summary>
        /// 试管ID
        /// </summary>
        [Required]
        [DefaultValue("Tube_Standard")]
        public string TubeId { get; set; }
        /// <summary>
        /// 松开程度
        /// </summary>
        public LidUncoverCReleaseLevel Level { get; set; }
    }
}
