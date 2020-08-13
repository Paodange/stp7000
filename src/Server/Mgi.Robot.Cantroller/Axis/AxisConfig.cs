using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Robot.Cantroller.Axis
{

    public class AxisConfig
    {
        /// <summary>
        /// 轴名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 轴编号
        /// </summary>
        public byte No { get; set; }

        /// <summary>
        /// 帧ID。(与CAN卡的帧ID一致)
        /// </summary>
        public byte FrameId { get; set; }

        /// <summary>
        /// 轴/电机的设置。(包含速度等，请由专业人员设置)
        /// </summary>
        public List<AxisSetting> Setting { get; set; }

        /// <summary>
        /// 是否开起误差检测。当值值为True时，AllowedDeviation有效。
        /// </summary>
        public bool IfCheckDeviation { get; set; }

        /// <summary>
        /// 允许的最大误差。（取绝对值）
        /// </summary>
        public int AllowedDeviation { get; set; }

        /// <summary>
        /// 运动到位或者其他操作的等待时间。请用毫秒
        /// </summary>
        public TimeSpan WaitTimeout { get; set; }

        /// <summary>
        /// 软限位最小值
        /// </summary>
        public int SoftLimitMin { get; set; }

        /// <summary>
        /// 软限位最大值
        /// </summary>
        public int SoftLimitMax { get; set; }

        /// <summary>
        /// 查询编码器实际位置的FrameType  不同的板卡之间此参数可能不一样  增加到配置中  如无此配置 默认209 兼容以前的板卡
        /// </summary>
        public byte EncoderPosFrameType { get; set; } = 209;
    }


    /// <summary>
    /// 轴配置
    /// </summary>
    public class AxisSetting
    {
        public byte Type { get; set; }

        public int Value { get; set; }
    }

}
