namespace Mgi.ALM.IOBoard
{
    public class SensorConfig
    {
        /// <summary>
        /// com端口名字
        /// </summary>
        public string ComName { get; set; }

        /// <summary>
        /// 端口通信波特率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 接收数据超时时间，在这个时间内没有接收到完整数据，单位s
        /// </summary>
        public int RevTimeOut { get; set; }
    }
}
