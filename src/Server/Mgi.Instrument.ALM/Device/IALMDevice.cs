using System;

namespace Mgi.Instrument.ALM.Device
{
    public interface IALMDevice
    {
        /// <summary>
        /// 
        /// </summary>
        string Id { get; }
        /// <summary>
        /// 
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 
        /// </summary>
        DeviceStatus Status { get; }
        /// <summary>
        /// 
        /// </summary>
        ALMDeviceType DeviceType { get; }
        /// <summary>
        /// 
        /// </summary>
        void Initialize();
        /// <summary>
        /// 
        /// </summary>
        void Close();

        /// <summary>
        /// 初始化顺序   值小的优先初始化
        /// </summary>
        int InitializeOrder { get; }
    }
}
