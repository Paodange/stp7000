using Mgi.ALM.Util.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mgi.Robot.Cantroller.Can
{
    /// <summary>
    /// 在DLL的基础上，增加了一些操作验证。此类暴露给UI使用
    /// CAN卡是可以并发使用的
    /// </summary>
    public sealed class DefaultCanController : ICanController
    {
        //private readonly ILog _logger;

        public const int ContentMaxLength = 8;
       
        public CanParameter Parameter { get; private set; }

        #region Construct

        public DefaultCanController(CanParameter parameter)
        {
            Parameter = parameter;

        }
        
        #endregion

        #region Impment SpCompnent       
        /// <summary>
        /// 只有初始化之后，才能读写操作，并且存在事件通知
        /// </summary>
        /// <exception cref="InvalidCanOperationException">如果初始化失败</exception>
        public void Initialize()
        {
            (this as ICanController).Start();
        }

        
        public void Close()
        {
            (this as ICanController).CloseDevice();
        }

        /// <summary>
        /// 打开设备，打开后可以查询一些状态信息
        /// </summary>
        /// <exception cref="InvalidCanOperationException">如果打开失败</exception>
        public void Open()
        {
            var CAN = this as ICanController;
            CAN.OpenCan();
            var config = new VciInitConfig()
            {
                AccMask = 0xFFFFFFFF,
                Mode = 0,
                Timing0 = 0x00,
                Timing1 = 0x14
            };
            CAN.InitCan(ref config);
            CAN.ClearBuffer();
        }
       
        #endregion


        #region Implement ICanController

        public void Write(IEnumerable<byte[]> datas)
        {
            var frames = datas.Select<byte[], VciCanFrame>(data => GenerateDefualtFrame(data, (byte)data.Count()))
                                .ToArray();

            (this as ICanController).Write(frames);
        }


        /// <summary>
        /// vco[0].ID = frameID;// 填写第一帧的ID
        //  vco[0].SendType = 0;// 正常发送
        //  vco[0].RemoteFlag = 0;// 数据帧
        //  vco[0].ExternFlag = 0;// 标准帧
        //  vco[0].DataLen = 1;// 数据长度1个字节
        //  vco[0].Data[0] = 0x66;// 数据0为0x66
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private VciCanFrame GenerateDefualtFrame(byte[] data, byte length)
        {
            var frame = new VciCanFrame()
            {
                ID = Parameter.FrameId,
                DataLen = length,
                SendType = 0,
                RemoteFlag = 0,
                ExternFlag = 0,
                Data = data //new byte[8]              
            };
            return frame;
        }
        

        public VciCanFrame Read()
        {
            var response = (this as ICanController).Read(1);
            //_logger.Debug($"Read: {response.First().Data.ToHexString()}");
            return response.First();
        }

        public byte[] ReadContents()
        {
            throw new NotImplementedException("-0xFF[1330664]0xEE-");
        }

        ICanController ICanController.Write(byte[] data, byte length)
        {
            if (data == null || length > ContentMaxLength || length == 0)
                throw new ArgumentException($"-0xFF[1330665]0xEE-data length must be in (0 {ContentMaxLength})");

            //_logger.Debug($"Write: {data?.ToHexString()}");
            var frame = GenerateDefualtFrame(data, length);
            (this as ICanController).Write(ref frame);
            return this;
        }

        public void ReloadConfiguration()
        {
            throw new NotImplementedException("-0xFF[1330666]0xEE-");
        }

        public void Binding(uint deviceType, uint deviceIndex, uint canIndex, TimeSpan ioTimeout, uint frameId)
        {
            Parameter = new CanParameter()
            {
                FrameId = frameId,
                DeviceType = deviceType,
                DeviceIndex = deviceIndex,
                CanIndex = canIndex,
                IoTimeout = (int)ioTimeout.TotalMilliseconds
            };
        }
        #endregion
    }
}
