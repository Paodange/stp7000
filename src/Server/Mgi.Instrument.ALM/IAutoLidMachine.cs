using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Services;
using Mgi.Instrument.ALM.Workflow;
using System;
using System.Collections.Generic;

namespace Mgi.Instrument.ALM
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAutoLidMachine
    {
        MachineStatus Status { get; }
        DeviceCollection Devices { get; }
        IWorkflowManager WorkflowManager { get; }
        IScriptEngine ScriptEngine { get; }
        /// <summary>
        /// 
        /// </summary>
        IALMLidUnCover LidUncover { get; }
        /// <summary>
        /// 
        /// </summary>
        IALMPipettes Pipettes { get; }
        /// <summary>
        /// 
        /// </summary>
        IALMRobot Robot { get; }

        IALMIOBoard IOBoard { get; }
        IZLimsMessageService ZLimsService { get; }
        IALMWorkflow Workflow { get; }
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<DeviceInitCompleteEventArgs> DeviceInitComplete;
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<DeviceInitErrorEventArgs> DeviceInitError;
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<MachineInitCompleteEventArgs> MachineInitComplete;
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<MethodExceptionEventArgs> MethodExecuteError;

        event EventHandler<MachineStatusChangedEventArgs> StatusChanged;
        /// <summary>
        /// 关闭退出时触发
        /// </summary>
        event EventHandler Shutdown;

        void Run(IALMWorkflow workflow, IEnumerable<TransferPlateInfo> transferPlateInfos, IEnumerable<MicroPlate> tipsContainer);
        /// <summary>
        /// 
        /// </summary>
        void PowerOn();
        /// <summary>
        /// 
        /// </summary>
        void PowerOff();
    }
    /// <summary>
    /// 
    /// </summary>
    public class DeviceInitCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IALMDevice Device { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public DeviceInitCompleteEventArgs(IALMDevice device)
        {
            Device = device;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class DeviceInitErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IALMDevice Device { get; }
        /// <summary>
        /// 
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="e"></param>
        public DeviceInitErrorEventArgs(IALMDevice device, Exception e)
        {
            Device = device;
            Exception = e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class MachineInitCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        public MachineInitCompleteEventArgs(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

    public class MachineStatusChangedEventArgs : EventArgs
    {
        public MachineStatus OldStatus { get; }
        public MachineStatus NewStatus { get; }
        public MachineStatusChangedEventArgs(MachineStatus oldStatus, MachineStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}
