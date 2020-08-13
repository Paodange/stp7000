using log4net;
using Mgi.Instrument.ALM.Action;
using Mgi.Instrument.ALM.Config;
using Mgi.Instrument.ALM.Util;
using Mgi.Robot.Cantroller;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Mgi.Instrument.ALM.Device.Real
{
    internal class ALMPipettes : AbstractPipette, IALMPipettes
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMPipettes");
        public ALMPipettes(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log, false)
        {

        }
    }
}
