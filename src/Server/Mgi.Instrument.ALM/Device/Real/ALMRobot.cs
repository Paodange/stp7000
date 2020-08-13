using Castle.DynamicProxy;
using log4net;
using Mgi.Epson.Robot.T3;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Config;
using Mgi.Instrument.ALM.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mgi.Instrument.ALM.Device.Real
{
    internal class ALMRobot : AbstractRobot, IALMRobot
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMRobot");
        public ALMRobot(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log)
        {
        }

        protected override void InitialComponents()
        {
            var gripperConfig = ConfigProvider.GetGripperConfig();
            if (gripperConfig.Simulated)
            {
                Gripper = ProxyGenerator.CreateInterfaceProxyWithTarget<IZimmaGripper>(
                     new SimulatedModbusGripper() { Name = "RobotGripper" },
                     new DeviceCommandInterceptor("SimulatedRobotGripper", WorkflowManager, log));
            }
            else
            {
                Gripper = ProxyGenerator.CreateInterfaceProxyWithTarget<IZimmaGripper>(
                    new ModbusGripper(gripperConfig) { Name = "RobotGripper" },
                    new DeviceCommandInterceptor("EpsonRobotGripper", WorkflowManager, log));
            }
            EpsonRobot = ProxyGenerator.CreateInterfaceProxyWithTarget<IEpsonRobot>(
                new EpsonRobot(ConfigProvider.GetRobotConfig(), log),
                new DeviceCommandInterceptor("EpsonRobot", WorkflowManager, log));
        }
    }
}
