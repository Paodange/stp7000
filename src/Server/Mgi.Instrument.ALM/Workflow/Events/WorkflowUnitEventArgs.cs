using Mgi.Instrument.ALM.Device;

namespace Mgi.Instrument.ALM.Workflow.Events
{
    public class WorkflowUnitEventArgs : PlateEventArgs
    {
        public PlatePosition Position { get; }
        public PipettePositions TargetPos { get; }
        public string TargetDeepPlateName { get; }
        public PlatePosition TargetPosition { get; }
        public WorkflowUnitEventArgs(string plateName, PlatePosition platePosition, string targetDeepPlateName, PipettePositions targetPos,
            PlatePosition targetPosition) : base(plateName)
        {
            Position = platePosition;
            TargetDeepPlateName = targetDeepPlateName;
            TargetPosition = targetPosition;
            TargetPos = targetPos;
        }
    }
}
