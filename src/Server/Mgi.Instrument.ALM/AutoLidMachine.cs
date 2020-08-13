using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Services;
using Mgi.Instrument.ALM.Util;
using Mgi.Instrument.ALM.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mgi.Instrument.ALM
{
    internal class AutoLidMachine : InstrumentBase, IAutoLidMachine
    {
        IEnumerable<IBackgroundService> BackgroundServices { get; }
        public IALMLidUnCover LidUncover
        {
            get
            {
                return FindFirst<IALMLidUnCover>(ALMDeviceType.LidUnCover);
            }
        }

        public IALMPipettes Pipettes
        {
            get
            {
                return FindFirst<IALMPipettes>(ALMDeviceType.Pipettes);
            }
        }

        public IALMRobot Robot
        {
            get
            {
                return FindFirst<IALMRobot>(ALMDeviceType.Robot);
            }
        }

        public IALMIOBoard IOBoard
        {
            get
            {
                return FindFirst<IALMIOBoard>(ALMDeviceType.IOBoard);
            }
        }

        public IZLimsMessageService ZLimsService
        {
            get
            {
                return BackgroundServices.FirstOrDefault(x => typeof(IZLimsMessageService).IsAssignableFrom(x.GetType())) as IZLimsMessageService;
            }
        }

        public IWorkflowManager WorkflowManager { get; }
        public IALMWorkflow Workflow { get; private set; }

        private MachineStatus status = MachineStatus.Idle;
        public MachineStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status != value)
                {
                    var s = status;
                    status = value;
                    OnStatusChanged(new MachineStatusChangedEventArgs(s, status));
                }
            }
        }

        public AutoLidMachine(DeviceCollection devices,
            IList<IBackgroundService> backgroundServices,
            IScriptEngine scriptEngine,
            IWorkflowManager workflowManager)
            : base(devices, scriptEngine)
        {
            WorkflowManager = workflowManager;
            BackgroundServices = backgroundServices;
            foreach (var inteceptor in GeneralInterceptor.AllFilters)
            {
                if (inteceptor is DeviceCommandInterceptor d)
                {
                    d.Error += DeviceCommand_Error;
                }
            }
        }

        public event EventHandler<MachineStatusChangedEventArgs> StatusChanged;
        public event EventHandler<DeviceInitCompleteEventArgs> DeviceInitComplete;
        public event EventHandler<DeviceInitErrorEventArgs> DeviceInitError;
        public event EventHandler<MachineInitCompleteEventArgs> MachineInitComplete;
        public event EventHandler Shutdown;
        public override void PowerOff()
        {
            if (Workflow != null)
            {
                Workflow.Stop();
            }
            OnShutdown(new EventArgs());
            foreach (var service in BackgroundServices)
            {
                service.Stop();
            }
            foreach (var device in Devices)
            {
                device.Close();
            }
        }

        public override void PowerOn()
        {
            bool hasError = false;
            string message = string.Empty;
            //Devices.AsParallel().ForAll(x =>
            //    {
            //        try
            //        {
            //            x.Initialize();
            //            OnDeviceInitComplete(new DeviceInitCompleteEventArgs(x));
            //        }
            //        catch (Exception e)
            //        {
            //            hasError = true;
            //            message = e.Message;
            //            OnDeviceInitError(new DeviceInitErrorEventArgs(x, e));
            //        }
            //    });
            foreach (var device in Devices.OrderBy(x => x.InitializeOrder))
            {
                try
                {
                    device.Initialize();
                    OnDeviceInitComplete(new DeviceInitCompleteEventArgs(device));
                }
                catch (Exception e)
                {
                    hasError = true;
                    message = e.Message;
                    OnDeviceInitError(new DeviceInitErrorEventArgs(device, e));
                    break;
                }
            }
            if (!hasError)
            {
                foreach (var service in BackgroundServices)
                {
                    service.Start();
                }
                Status = MachineStatus.Idle;
            }
            OnMachineInitComplete(new MachineInitCompleteEventArgs(!hasError, message));
        }

        private void DeviceCommand_Error(object sender, MethodExceptionEventArgs e)
        {
            OnMethodException(sender, e);
        }

        protected virtual void OnDeviceInitComplete(DeviceInitCompleteEventArgs e)
        {
            DeviceInitComplete?.Invoke(this, e);
        }

        protected virtual void OnDeviceInitError(DeviceInitErrorEventArgs e)
        {
            DeviceInitError?.Invoke(this, e);
        }

        protected virtual void OnMachineInitComplete(MachineInitCompleteEventArgs e)
        {
            MachineInitComplete?.Invoke(this, e);
        }

        protected virtual void OnShutdown(EventArgs e)
        {
            Shutdown?.Invoke(this, e);
        }
        protected virtual void OnStatusChanged(MachineStatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }
        public void Run(IALMWorkflow workflow, IEnumerable<TransferPlateInfo> transferPlateInfos, IEnumerable<MicroPlate> tipsContainer)
        {
            try
            {
                Workflow = workflow;
                Status = MachineStatus.Running;
                Workflow.WorkflowPaused += (o, e) => Status = MachineStatus.Paused;
                workflow.WorkflowResume += (o, e) => Status = MachineStatus.Running;
                Workflow.Start(transferPlateInfos, tipsContainer);
            }
            finally
            {
                Status = MachineStatus.Idle;
            }
        }
    }
}
