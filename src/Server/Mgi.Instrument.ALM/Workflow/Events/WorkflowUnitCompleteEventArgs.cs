using Mgi.Instrument.ALM.Device;

namespace Mgi.Instrument.ALM.Workflow.Events
{
    public class WorkflowUnitCompleteEventArgs : WorkflowUnitEventArgs
    {
        public string Barcode { get; }
        public bool BarcodeReadFail { get; }
        public WorkflowUnitCompleteEventArgs(string plateName, PlatePosition sourcePositon, string targetDeepPlateName, PipettePositions targetPos,
            PlatePosition targetPosition, string barcode)
            : base(plateName, sourcePositon, targetDeepPlateName, targetPos, targetPosition)
        {

            BarcodeReadFail = string.IsNullOrWhiteSpace(barcode) || barcode.StartsWith("ADDRESS:");
            if (BarcodeReadFail)
            {
                Barcode = $"({barcode?.Replace("ADDRESS:", "")})";
            }
            else
            {
                Barcode = barcode;
            }
        }
    }

    public class WorkflowUnitUnAspirateCompleteEventAgrs : WorkflowUnitCompleteEventArgs
    {
        public WorkflowUnitUnAspirateCompleteEventAgrs(string plateName, PlatePosition sourcePositon, string targetDeepPlateName, PipettePositions targetPos,
             PlatePosition targetPosition, string barcode)
            : base(plateName, sourcePositon, targetDeepPlateName, targetPos, targetPosition, barcode)
        {

        }
    }
}
