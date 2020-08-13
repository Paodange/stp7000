namespace Mgi.Robot.Cantroller.Can
{
    public class CanParameter
    {
        /// <summary>
        /// 为此Can指定的名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设备类型号
        /// </summary>
        public uint DeviceType { get; set; }

        /// <summary>
        /// 设备索引号
        /// </summary>
        public uint DeviceIndex { get; set; }

        /// <summary>
        /// CAN 口索引
        /// </summary>
        public uint CanIndex { get; set; } 

        /// <summary>
        /// 保留参数，通常为0。特例：当设备为 CANET-UDP 时，此参数表示要打开的本地端
        /// 口号， 建议在 5000 到 40000 范围内取值。 当设备为 CANET-TCP 时， 此参数固定为 0。
        /// </summary>
        public uint Reserved { get; set; }

        /// <summary>
        /// 等待读取的最长时间 ms
        /// </summary>
        public int IoTimeout { get; set; }

        public uint FrameId { get; set; }
    }
}
