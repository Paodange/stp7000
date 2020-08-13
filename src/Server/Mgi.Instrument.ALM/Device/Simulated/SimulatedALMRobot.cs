using log4net;
using Mgi.Epson.Robot.T3;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Device.Simulated
{
    internal class SimulatedALMRobot : AbstractRobot, IALMRobot
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMRobot");

        public SimulatedALMRobot(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log)
        {

        }
        protected override void InitialComponents()
        {
            Gripper = ProxyGenerator.CreateInterfaceProxyWithTarget<IZimmaGripper>(
                new SimulatedModbusGripper(),
                new DeviceCommandInterceptor("EpsonRobotGripper", WorkflowManager, log));
            EpsonRobot = ProxyGenerator.CreateInterfaceProxyWithTarget<IEpsonRobot>(
                new SimulatedEpsonRobot(),
                new DeviceCommandInterceptor("EpsonRobot", WorkflowManager, log));
        }
    }
}
