using System;
using System.Collections.Generic;

namespace Mgi.Robot.Cantroller.Can
{
    /// <summary>
    /// Exposed to outside.
    /// It can't implent IHardware, it need too much parameters
    /// </summary>
    public interface ICanController
    {
        /// <summary>
        /// CAN卡参数
        /// </summary>
        CanParameter Parameter { get; }

        void Open();

        /// <summary>
        /// 在读写CAN之前，先绑定CAN卡参数
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <param name="ioTimeout"></param>
        void Binding(uint deviceType, uint deviceIndex, uint canIndex, TimeSpan ioTimeout, uint frameId);

        void Initialize();

        void Close();

        void Write(IEnumerable<byte[]> datas);

        /// <summary>
        /// 读取一帧
        /// </summary>
        /// <returns></returns>
        VciCanFrame Read();

        /// <summary>
        /// 读取CAN Frame内容
        /// </summary>
        /// <returns></returns>
        byte[] ReadContents();

        /// <summary>
        /// 写一帧
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        ICanController Write(byte[] data, byte length);
    }
}
