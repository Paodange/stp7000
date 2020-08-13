using log4net;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Device.Simulated
{
    internal class SimulatedALMPipettes : AbstractPipette, IALMPipettes
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMPipettes");
        public SimulatedALMPipettes(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log, true)
        {

        }
    }
}
