using log4net;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Device.Simulated
{
    internal class SimulatedIOBoard : AbstractIOBoard
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMIOBoard");
        public SimulatedIOBoard(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log, true)
        {

        }

        public override DoorState GetFrontDoorState()
        {
            return DoorState.Closed;
        }
    }
}
