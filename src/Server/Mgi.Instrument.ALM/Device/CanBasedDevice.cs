using log4net;
using Mgi.Instrument.ALM.Axis;
using Mgi.Instrument.ALM.Util;
using Mgi.Robot.Cantroller;
using Mgi.Robot.Cantroller.Axis;
using Mgi.Robot.Cantroller.Can;
using System;
using System.Collections.Generic;

namespace Mgi.Instrument.ALM.Device
{
    /// <summary>
    /// 基于CAN卡控制的设备的基类
    /// </summary>
    internal abstract class CanBasedDevice : AbstractDevice
    {
        public Dictionary<string, IALMAxis> Axises { get; private set; }
        public List<ICanController> CanControllers { get; private set; }
        bool initialized = false;
        readonly ILog log;
        public bool Simulated { get; }
        public IWorkflowManager WorkflowManager { get; }
        public CanBasedDevice(bool simulated, List<RobotConfig> robotConfigs, IConfigProvider configProvider, ILog log, IWorkflowManager workflowManager)
            : base(configProvider)
        {
            this.log = log;
            Simulated = simulated;
            WorkflowManager = workflowManager;
            InitializeRobots(robotConfigs);
        }

        private void InitializeRobots(List<RobotConfig> robotConfigs)
        {
            Axises = new Dictionary<string, IALMAxis>();
            CanControllers = new List<ICanController>();
            foreach (var robotConfig in robotConfigs)
            {
                var parameter = new CanParameter()
                {
                    CanIndex = robotConfig.CanIndex,
                    DeviceIndex = robotConfig.DeviceIndex,
                    DeviceType = robotConfig.DeviceType,
                    FrameId = robotConfig.FrameId,
                    IoTimeout = (int)robotConfig.CanTimeout.TotalMilliseconds
                };
                ICanController can;
                if (Simulated || robotConfig.Simulated)
                {
                    can = ProxyGenerator.CreateInterfaceProxyWithTarget<ICanController>(new SimulatedCanContorller(parameter, log),
                        new DeviceCommandInterceptor($"{parameter.Name}(Simulated)", WorkflowManager, log));
                }
                else
                {
                    can = ProxyGenerator.CreateInterfaceProxyWithTarget<ICanController>(new DefaultCanController(parameter, log),
                        new DeviceCommandInterceptor($"{parameter.Name}", WorkflowManager, log));
                }
                CanControllers.Add(can);
                foreach (var axisConfig in robotConfig.AxisConfig)
                {
                    IALMAxis axis;
                    if (Simulated || robotConfig.Simulated)
                    {
                        axis = ProxyGenerator.CreateInterfaceProxyWithTarget<IALMAxis>(new SimulatedALMAxis(axisConfig, can, log),
                             new DeviceCommandInterceptor($"Axis_{axisConfig.Name}(Simulated)", WorkflowManager, log));
                    }
                    else
                    {
                        axis = ProxyGenerator.CreateInterfaceProxyWithTarget<IALMAxis>(new ALMAxis(axisConfig, can, log),
                             new DeviceCommandInterceptor($"Axis_{axisConfig.Name}", WorkflowManager, log));
                    }
                    Axises.Add(axisConfig.Name, axis);
                }
            }
        }

        public override void Close()
        {
            try
            {
                foreach (var can in CanControllers)
                {
                    can.ClearBuffer();
                }
                foreach (var axis in Axises.Values)
                {
                    axis.StopAsync();
                }
            }
            catch (Exception ex)
            {
                log.Warn("Close Can Error", ex);
            }
            initialized = false;
            log.Info("Close success");
        }
        public override void Initialize()
        {
            if (initialized) return;
            CheckConfig();
            foreach (var can in CanControllers)
            {
                can.Binding(can.Parameter.DeviceType, can.Parameter.DeviceIndex, can.Parameter.CanIndex,
                                       TimeSpan.FromMilliseconds(can.Parameter.IoTimeout), can.Parameter.FrameId);
                can.Open();
                can.Initialize();
            }
            foreach (var item in Axises)
            {
                item.Value.UseConfigSetting();
            }
            HomeAll();
            log.Info("Initialize success");
            initialized = true;
        }

        /// <summary>
        /// home时序有要求  需要在子类根据具体设备要求实现
        /// </summary>
        public abstract void HomeAll();

        /// <summary>
        /// 检查配置是否正确 在初始化前会调用 不正确时 需要抛出异常 
        /// </summary>
        protected abstract void CheckConfig();
    }
}
