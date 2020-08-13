using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Mgi.Epson.Robot.T3
{
    public class RobotPath
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RobotAction Action { get; set; }
        public string Position { get; set; }
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        /// <summary>
        /// Z轴 (垂直地面方向偏移)
        /// </summary>
        public double ZOffset { get; set; }

        /// <summary>
        /// 字符串点位转为整形点位  机械臂内部存整型  为方便阅读程序存字符串
        /// </summary>
        /// <returns></returns>
        public int GetActualPos()
        {
            if (string.IsNullOrWhiteSpace(Position))
            {
                throw new Exception("Point cannot be null or empty");
            }
            int index = -1;
            for (int i = 0; i < Position.Length; i++)
            {
                if (char.IsDigit(Position[i]))
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                throw new Exception($"Invalid point:{Position}");
            }
            var s = Position.Substring(index);
            if (int.TryParse(s, out var p))
            {
                return p;
            }
            throw new Exception($"Invalid point:{Position}");
        }
    }
}
