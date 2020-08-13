using log4net;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Device.Real
{
    internal class ALMIOBoard : AbstractIOBoard
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMIOBoard");
        public ALMIOBoard(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log, false)
        {
        }
    }
}
