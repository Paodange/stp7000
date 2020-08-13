using Mgi.Instrument.ALM.Workflow.Events;
using System;
using System.Threading;

namespace Mgi.Instrument.ALM.Workflow
{
    public class SimulatedWorkflow : AbstractALMWorkflow
    {

        public SimulatedWorkflow(IAutoLidMachine alm)
            : base(alm)
        {
        }
        Random r = new Random();
        protected override void Run(string tubeId, double volume)
        {
            OnWorkflowBegin(new EventArgs());
            foreach (var sample in Samples)
            {
                OnUnitBegin(new WorkflowUnitEventArgs(CurrentPlateName, sample.Position, DeepPlateName, GetTargetDeepPlatePostion(), sample.Position));
                Thread.Sleep(2000);

                if (r.Next(1, 100) > 80)
                {
                    OnUnitError(new WorkflowUnitErrorEventArgs(CurrentPlateName, sample.Position, DeepPlateName, Device.PipettePositions.POS3, sample.Position, "12345678", new Exception("fdsfds")));
                }
                else
                {
                    Thread.Sleep(1000);
                    OnUnitComplete(new WorkflowUnitCompleteEventArgs(CurrentPlateName, sample.Position, DeepPlateName, Device.PipettePositions.POS3, sample.Position, "12345678"));
                }
            }
            OnWorkflowEnd(new EventArgs());
        }
    }
}
