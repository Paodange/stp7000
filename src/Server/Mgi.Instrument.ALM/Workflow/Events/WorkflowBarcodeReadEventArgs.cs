using Mgi.Instrument.ALM.Device;

namespace Mgi.Instrument.ALM.Workflow.Events
{

    public class WorkflowBarcodeReadEventArgs : WorkflowUnitEventArgs
    {
        public string Barcode { get; }
        public WorkflowBarcodeReadEventArgs(string plateName, PlatePosition platePosition, string targetDeepPlateName, PipettePositions targetPos,
            PlatePosition targetPosition, string barcode)
            : base(plateName, platePosition, targetDeepPlateName, targetPos, targetPosition)
        {
            Barcode = barcode;
        }
    }
}
