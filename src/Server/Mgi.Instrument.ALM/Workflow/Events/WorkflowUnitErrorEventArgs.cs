using System;
using Mgi.Instrument.ALM.Device;

namespace Mgi.Instrument.ALM.Workflow.Events
{
    public class WorkflowUnitErrorEventArgs : WorkflowUnitEventArgs
    {

        public string Barcode { get; }
        public Exception Exception { get; }
        public WorkflowUnitErrorEventArgs(string plateName, PlatePosition sourcePositon, string targetDeepPlateName, PipettePositions targetPos,
            PlatePosition targetPosition, string barcode, Exception ex)
            : base(plateName, sourcePositon, targetDeepPlateName, targetPos, targetPosition)
        {
            Barcode = barcode;
            Exception = ex;

        }
    }
}
