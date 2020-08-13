namespace Mgi.ALM.IOBoard
{
    public interface IOnboardHardware
    {
        /// <summary>
        /// 打开或者连接硬件.具体实现可能是阻塞的
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭硬件.具体实现可能是阻塞的
        /// </summary>
        void Close();

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();

        void Stop();

        void ReOpen();

        void ReInitialize();

        /// <summary>
        /// 命令码跟功能值
        /// </summary>
        /// <param name="value">value包含寻址与值</param>
        /// <exception cref="Sp200Exception">当Out发生错误时</exception>
        void GpioOut(uint socket, uint value);

        /// <summary>
        /// 设置LED灯带的RGB值
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        void Color(byte r, byte g, byte b);

        /// <summary>
        /// 设置蜂鸣器的时长
        /// </summary>
        /// <param name="time"></param>
        void SetBuzzerDuration(int duration);

        /// <summary>
        /// 设置蜂鸣器的波特率
        /// </summary>
        /// <param name="rate"></param>
        void SetBuzzerBaud(int baud);

        uint GpioIn(uint value);

        void HoodSpeed(int speed);

        string GetVersion();

        int GetVolume();

        /// <summary>
        /// 颜色闪烁
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="onTime"></param>
        /// <param name="offTime"></param>
        void FlashColor(byte r, byte g, byte b, ushort onTime, ushort offTime);

        /// <summary>
        /// 蜂鸣器间隔响
        /// </summary>
        /// <param name="onTime"></param>
        /// <param name="offTime"></param>
        void FlashBuzzer(ushort onTime, ushort offTime);
    }
}
