using System;

namespace Mgi.Instrument.ALM.Workflow.Events
{
    public class WorkflowTransferPlateEventAgrs : EventArgs
    {
        public TransferPlateInfo TransferPlateInfo { get; }
        public WorkflowTransferPlateEventAgrs(TransferPlateInfo transferPlateInfo)
        {
            TransferPlateInfo = transferPlateInfo;
        }
    }
}
