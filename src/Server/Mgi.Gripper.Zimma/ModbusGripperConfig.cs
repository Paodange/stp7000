namespace Mgi.Gripper.Zimma
{
    public class ModbusGripperConfig
    {
        public string IP { get; set; }
        public ushort Port { get; set; }
        public ushort WriteChannelId { get; set; }
        public ushort ReadChannelId { get; set; }
        /// <summary>
        /// 抓取延迟 夹紧后等待时间
        /// </summary>
        public int GraspDelay { get; set; } = 500;
        /// <summary>
        /// 是否开启超阈值检测
        /// </summary>
        public bool ToleranceCheck { get; set; } = true;
        /// <summary>
        /// 是否模拟
        /// </summary>
        public bool Simulated { get; set; } = false;
    }
}
