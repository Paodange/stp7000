using Mgi.Epson.Robot.T3;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Mgi.Instrument.ALM.Config
{
    public class LocationMap
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RobotLocation Location { get; set; }
        /// <summary>
        /// 是否使用矩阵
        /// </summary>
        public bool UsePallet { get; set; }


        /// <summary>
        /// Z轴 (垂直地面方向偏移)
        /// </summary>
        public double ZOffset { get; set; }
        /// <summary>
        /// X轴偏移
        /// </summary>
        public double XOffset { get; set; }

        public List<RobotPath> GraspGoPaths { get; set; }
        public List<RobotPath> GraspBackPaths { get; set; }
        public List<RobotPath> LoosenGoPaths { get; set; }
        public List<RobotPath> LoosenBackPaths { get; set; }
    }
}
