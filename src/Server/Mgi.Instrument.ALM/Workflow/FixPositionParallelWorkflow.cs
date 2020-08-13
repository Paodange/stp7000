using System.Threading.Tasks;
using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Workflow.Events;

namespace Mgi.Instrument.ALM.Workflow
{
    /// <summary>
    /// 试管位置与深孔板位置一一对应的流程
    /// </summary>
    public class FixPositionParallelWorkflow : ParallelWorkflow
    {
        public FixPositionParallelWorkflow(IAutoLidMachine alm) : base(alm)
        {

        }

        protected override async Task UnAspirateAndUnloadTipsAsync(WorkunitSampleInfo info)
        {
            await Task.Run(() =>
            {
                if (info.Sample2 != null
                    && info.ContainerPos1.Row == info.ContainerPos2.Row
                    && !info.Sample1ProcessFail
                    && !info.Sample2ProcessFail)
                {
                    Alm.Pipettes.MoveTo(info.TargetDeepPlatePosition, info.Sample1.Position.Row, info.Sample1.Position.Column, info.Sample2.Position.Column);
                    Alm.Pipettes.UnAspirateBoth(info.TargetDeepPlatePosition);
                    OnUnitUnAspirateComplete(new WorkflowUnitUnAspirateCompleteEventAgrs(CurrentPlateName, info.Sample1.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos1, info.Barcode1));
                    OnUnitUnAspirateComplete(new WorkflowUnitUnAspirateCompleteEventAgrs(CurrentPlateName, info.Sample2.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos2, info.Barcode2));
                    Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                    Alm.Pipettes.UnloadTips();
                }
                else
                {
                    if (info.Sample2 == null)
                    {
                        PipetteChannels channel = PipetteChannels.A;
                        if (!info.Sample1ProcessFail)
                        {
                            Alm.Pipettes.MoveTo(info.TargetDeepPlatePosition, info.ContainerPos1.Row, info.ContainerPos1.Column);
                            Alm.Pipettes.UnAspirate(channel, info.TargetDeepPlatePosition);
                            OnUnitUnAspirateComplete(new WorkflowUnitUnAspirateCompleteEventAgrs(CurrentPlateName, info.Sample1.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos1, info.Barcode1));
                        }
                        Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                        Alm.Pipettes.UnLoadTip(channel);
                    }
                    else
                    {
                        if (!info.Sample1ProcessFail)
                        {
                            Alm.Pipettes.MoveTo(info.TargetDeepPlatePosition, info.ContainerPos1.Row, info.ContainerPos1.Column);
                            Alm.Pipettes.UnAspirate(PipetteChannels.A, info.TargetDeepPlatePosition);
                            OnUnitUnAspirateComplete(new WorkflowUnitUnAspirateCompleteEventAgrs(CurrentPlateName, info.Sample1.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos1, info.Barcode1));
                        }
                        if (!info.Sample2ProcessFail)
                        {
                            Alm.Pipettes.MoveTo(info.TargetDeepPlatePosition, info.ContainerPos2.Row, info.ContainerPos2.Column - 1);
                            Alm.Pipettes.UnAspirate(PipetteChannels.B, info.TargetDeepPlatePosition);
                            OnUnitUnAspirateComplete(new WorkflowUnitUnAspirateCompleteEventAgrs(CurrentPlateName, info.Sample2.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos2, info.Barcode2));
                        }
                        Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                        Alm.Pipettes.UnloadTips();
                    }
                }
            });
        }

        protected override WorkunitSampleInfo GetNextTwoSampleInfo()
        {
            lock (syncLock)
            {
                var sample1 = GetNextSample();
                var sample2 = GetNextSample();
                var pos1 = default(PlatePosition);
                var pos2 = default(PlatePosition);
                TipInfo tipInfo1 = null, tipInfo2 = null;
                if (sample1 != null)
                {
                    pos1 = sample1.Position;
                    tipInfo1 = GetNextTipInfo();
                    if (sample2 != null)
                    {
                        pos2 = sample2.Position;
                        tipInfo2 = GetNextTipInfo();
                    }
                    var unit = GetWorkUnit();
                    var sliderPositions = GetSliderPositions(unit);
                    var info = new WorkunitSampleInfo()
                    {
                        ContainerPos1 = pos1,
                        ContainerPos2 = pos2,
                        Sample1 = sample1,
                        Sample2 = sample2,
                        AssignedUnit = unit,
                        SliderRobotPostion1 = sliderPositions.Item1,
                        SliderRobotPostion2 = sliderPositions.Item2,
                        SliderPos1 = SliderSamplePos.POS1,
                        SliderPos2 = SliderSamplePos.POS2,
                        TargetSamplePosition = unit == LidUncoverUnits.A ? PipettePositions.POS5 : PipettePositions.POS6,
                        TipInfo1 = tipInfo1,
                        TipInfo2 = tipInfo2,
                        TargetDeepPlatePosition = GetTargetDeepPlatePostion()
                    };
                    return info;
                }
                return null;
            }
        }
    }
}
