using Mgi.ALM.Util.Extension;
using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Workflow.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mgi.Instrument.ALM.Workflow
{
    public class ParallelWorkflow : AbstractALMWorkflow
    {
        SemaphoreSlim slim;
        public ParallelWorkflow(IAutoLidMachine _alm)
            : base(_alm)
        {
        }

        protected override void Run(string tubeId, double volume)
        {
            slim = new SemaphoreSlim(1, 2);
            var workUnitRstEvents = new Dictionary<LidUncoverUnits, AutoResetEvent>
                    {
                        { LidUncoverUnits.A, new AutoResetEvent(true) },
                        { LidUncoverUnits.B, new AutoResetEvent(true) }
                    };
            var areNextTask = new AutoResetEvent(false);
            var arePipette = new AutoResetEvent(true);
            var areRobot = new AutoResetEvent(true);
            List<Task> tasks = new List<Task>();

            var unloadTipsTask = UnLoadTipAsync();
            var resetRTask1 = ResetRAxisAsync(LidUncoverUnits.A);
            var resetRTask2 = ResetRAxisAsync(LidUncoverUnits.B);
            Task.WaitAll(resetRTask1, resetRTask2, unloadTipsTask);

            while(true) 
            {
                var info = GetNextTwoSampleInfo();
                if (info == null)
                {
                    break;
                }
                log.Info($"SampleInfo:{info.ToJsonString()}");
                if (WorkflowManager.Status == WorkflowStatus.Stopped)
                {
                    break;
                }
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await ProcessSampleInfoAsync(info, tubeId, volume, workUnitRstEvents[info.AssignedUnit], arePipette, areRobot, areNextTask);
                    }
                    finally
                    {
                        areNextTask.Set();
                        slim.Release();            // 可以进入下一轮循环
                    }
                }));
                areNextTask.WaitOne();
                slim.Wait();
            }
            Task.WaitAll(tasks.ToArray());
        }


        protected virtual async Task ProcessSampleInfoAsync(WorkunitSampleInfo info, string tubeId, double volume, AutoResetEvent areWorkUnit, AutoResetEvent arePipette,
            AutoResetEvent areRobot, AutoResetEvent areNextTask)
        {
            areRobot.WaitOne();
            await TransferToSliderAsync(info, tubeId);  // 转移试管到Slider
            areRobot.Set();
            areNextTask.Set();  //下一个线程可以开始工作

            areWorkUnit.WaitOne();
            Task tTightenGripper = TightenGrippersAsync(tubeId, info.AssignedUnit);
            Task tMoveToLiduncover1 = MoveToLiduncoverAsync(info.AssignedUnit);  //slider移动到开盖设备
            await Task.WhenAll(tTightenGripper, tMoveToLiduncover1);

            // 开盖并扫码的同时 移液器扎吸头并移动到吸液位置
            Task tUncover = UncoverAndScanAsync(info, tubeId);  // 开盖并扫码

            arePipette.WaitOne();
            Task tLoadTips = LoadTipsAndMoveToSampleAsync(info);   // 移液器扎吸头
            await tUncover;
            //Task tResetR = ResetRAxisAsync(info.AssignedUnit);
            Task tMoveToPipptte = MoveToPipetteAsync(info.AssignedUnit);
            // 等待扎吸头完成 试管转移到吸液位置
            await Task.WhenAll(tLoadTips, tMoveToPipptte);

            await AspirateAsync(info, tubeId, volume);  //吸液

            // 吸完液后，移液器转移到深孔板的同时 盖回盖子并转移回Hotel
            Task tUnAspirateAndUnloadTips = Task.Run(async () =>
            {
                await UnAspirateAndUnloadTipsAsync(info);
                arePipette.Set();
            });

            // 盖回盖子并转移回Hotel
            Task tConverAndTrasnferToHotel = Task.Run(async () =>
            {
                if (!info.Sample1ProcessFail || (info.Sample2 != null && !info.Sample2ProcessFail)) // 有一个没吸液失败  则要进行盖回盖子操作
                {
                    Task tMoveToLiduncover2 = MoveToLiduncoverAsync(info.AssignedUnit);
                    Task tReleaseFailedGripper = ReleaseFailedGripperAsync(info);
                    await Task.WhenAll(tMoveToLiduncover2, tReleaseFailedGripper);
                    await CoverAsync(info, tubeId);
                }
                var tResetR = ResetRAxisAsync(info.AssignedUnit);
                await MoveToRobotAsync(info.AssignedUnit);
                areRobot.WaitOne();
                await TransferToHotelAsync(info, tubeId);
                areRobot.Set();
                await tResetR;
                areWorkUnit.Set();
            });
            await Task.WhenAll(tUnAspirateAndUnloadTips, tConverAndTrasnferToHotel);
        }

        /// <summary>
        ///  试管从Hotel转移到Slider
        /// </summary>
        /// <param name="info"></param>
        /// <param name="tubeId"></param>
        /// <returns></returns>
        protected virtual async Task TransferToSliderAsync(WorkunitSampleInfo info, string tubeId)
        {
            OnUnitBegin(new WorkflowUnitEventArgs(CurrentPlateName, info.Sample1.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos1));
            Task tMoveToRobotAndReleaseC = MoveToRobotAndReleaseCAsync(tubeId, info.AssignedUnit);
            //Task tReleaseCAxis = Task.Run(() => Alm.LidUncover.ReleaseCAxis(tubeId, info.AssignedUnit, LidUncoverCReleaseLevel.ForLoosen));
            Task tGraspFirst = Task.Run(() =>
            {
                Alm.Robot.Grasp(tubeId, GetHotelRobotLocation(), info.Sample1.Position.Row, info.Sample1.Position.Column);
                Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos1, SliderSamplePosState.Empty); //检查传感器
            });
            await Task.WhenAll(tMoveToRobotAndReleaseC, tGraspFirst);
            //Alm.Robot.Loosen(tubeId, info.SliderPostion1);
            Alm.Robot.LoosenOnly(tubeId, info.SliderRobotPostion1);
            Alm.LidUncover.TightenCAxis(tubeId, info.AssignedUnit);
            Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos1, SliderSamplePosState.Occupied); //检查传感器
            Alm.Robot.LoosenGoBack(info.SliderRobotPostion1);
            if (info.Sample2 != null)
            {
                OnUnitBegin(new WorkflowUnitEventArgs(CurrentPlateName, info.Sample2.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos2));
                Task tReleaseCAxis2 = Task.Run(() => Alm.LidUncover.ReleaseCAxis(tubeId, info.AssignedUnit, LidUncoverCReleaseLevel.ForLoosen));
                Task tGraspSecond = Task.Run(() =>
                 {
                     Alm.Robot.Grasp(tubeId, GetHotelRobotLocation(), info.Sample2.Position.Row, info.Sample2.Position.Column);
                     Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos2, SliderSamplePosState.Empty); //检查传感器
                 });
                await Task.WhenAll(tReleaseCAxis2, tGraspSecond);
                //Alm.Robot.Loosen(tubeId, info.SliderPostion2);
                Alm.Robot.LoosenOnly(tubeId, info.SliderRobotPostion2);
                Alm.LidUncover.TightenCAxis(tubeId, info.AssignedUnit);
                Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos2, SliderSamplePosState.Occupied); //检查传感器
                Alm.Robot.LoosenGoBack(info.SliderRobotPostion2);
            }
            //Alm.LidUncover.ReleaseCAxis(tubeId, info.AssignedUnit, LidUncoverCReleaseLevel.ForGrasp);
        }
        protected virtual async Task UncoverAndScanAsync(WorkunitSampleInfo info, string tubeId)
        {
            await Task.Run(() =>
           {
               Alm.LidUncover.SliderMoveTo(info.AssignedUnit, SliderPositionEnum.LidUncover);
               var barcodeGroup = Alm.LidUncover.UnCoverAndScan(tubeId, info.AssignedUnit, info.Sample2 == null ? LidUncoverGripper.A : LidUncoverGripper.Both); //拔盖
               if (barcodeGroup != null && barcodeGroup.Count() == 2)
               {
                   info.Barcode1 = barcodeGroup.ElementAt(0);
                   info.Barcode2 = barcodeGroup.ElementAt(1);
               }
               if (string.IsNullOrWhiteSpace(info.Barcode1))
               {
                   info.Barcode1 = "ADDRESS:" + info.Sample1.Position.ToString();
               }
               Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos1, SliderSamplePosState.Occupied); //检查传感器
               if (info.Sample2 != null)
               {
                   if (string.IsNullOrWhiteSpace(info.Barcode2))
                   {
                       info.Barcode2 = "ADDRESS:" + info.Sample2.Position.ToString();
                   }
                   Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos2, SliderSamplePosState.Occupied); //检查传感器
               }
           });
        }
        protected virtual async Task MoveToRobotAndReleaseCAsync(string tubeId, LidUncoverUnits workUnit)
        {
            await Task.Run(() =>
            {
                Alm.LidUncover.SliderMoveTo(workUnit, SliderPositionEnum.RobotPosition);
                Alm.LidUncover.ReleaseCAxis(tubeId, workUnit, LidUncoverCReleaseLevel.ForLoosen);
            });
        }
        protected virtual async Task TightenAndMoveToLiduncoverAsync(string tubeId, LidUncoverUnits workUnit)
        {
            await Task.Run(() =>
            {
                Alm.LidUncover.TightenCAxis(tubeId, workUnit);
                Alm.LidUncover.SliderMoveTo(workUnit, SliderPositionEnum.RobotPosition);
            });
        }
        protected virtual async Task MoveToRobotAsync(LidUncoverUnits workUnit)
        {
            await Task.Run(() => Alm.LidUncover.SliderMoveTo(workUnit, SliderPositionEnum.RobotPosition));
        }
        protected virtual async Task MoveToLiduncoverAsync(LidUncoverUnits workUnit)
        {
            await Task.Run(() => Alm.LidUncover.SliderMoveTo(workUnit, SliderPositionEnum.LidUncover));
        }
        protected virtual async Task MoveToPipetteAsync(LidUncoverUnits workUnit)
        {
            await Task.Run(() => Alm.LidUncover.SliderMoveTo(workUnit, SliderPositionEnum.Pipettes));
        }
        protected virtual async Task LoadTipsAndMoveToSampleAsync(WorkunitSampleInfo info)
        {
            await Task.Run(() =>
            {
                if (info.Sample2 != null
                    && info.TipInfo1.Pos == info.TipInfo2.Pos
                    && info.TipInfo1.Position.Row == info.TipInfo2.Position.Row
                    && (info.TipInfo1.Position.Column - info.TipInfo2.Position.Column) == -1)
                {
                    // 两个通道
                    Alm.Pipettes.MoveTo(info.TipInfo1.Pos, info.TipInfo1.Position.Row, info.TipInfo1.Position.Column);
                    Alm.Pipettes.LoadTips(info.TipInfo1.Pos);
                    OnLoadTipComplete(info.TipInfo1);
                    OnLoadTipComplete(info.TipInfo2);
                }
                else
                {
                    // 单通道  通道A无法到达 POS2最后一列   通道B 无法到达POS1第一列。   同时需要考虑排液的位置，假如到不了排液的位置
                    // 如果扎吸头只能用A通道，比如第一个吸头盒的第一列，但排液只能用B通道，在
                    if (info.Sample2 == null)
                    {
                        Alm.Pipettes.MoveTo(info.TipInfo1.Pos, info.TipInfo1.Position.Row, info.TipInfo1.Position.Column);
                        Alm.Pipettes.LoadTip(PipetteChannels.A, info.TipInfo1.Pos);
                        OnLoadTipComplete(info.TipInfo1);
                    }
                    else
                    {
                        Alm.Pipettes.MoveTo(info.TipInfo1.Pos, info.TipInfo1.Position.Row, info.TipInfo1.Position.Column);
                        Alm.Pipettes.LoadTip(PipetteChannels.A, info.TipInfo1.Pos);
                        Alm.Pipettes.MoveTo(info.TipInfo2.Pos, info.TipInfo2.Position.Row, info.TipInfo2.Position.Column - 1);
                        Alm.Pipettes.LoadTip(PipetteChannels.B, info.TipInfo2.Pos);
                        OnLoadTipComplete(info.TipInfo1);
                        OnLoadTipComplete(info.TipInfo2);
                    }
                }
                Alm.Pipettes.MoveTo(info.TargetSamplePosition, 1, 1);
            });
        }
        protected virtual async Task AspirateAsync(WorkunitSampleInfo info, string tubeId, double volume)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (info.Sample2 != null)
                    {
                        Alm.Pipettes.AspirateBoth(tubeId, info.TargetSamplePosition, volume);
                    }
                    else
                    {
                        Alm.Pipettes.Aspirate(tubeId, PipetteChannels.A, info.TargetSamplePosition, volume);
                    }
                }
                catch (AspirateAccuracyCheckFailException ex) //吸液丢步异常  可以认为盖子没打开 
                {
                    if (ex.Channel == PipetteChannels.Both)
                    {
                        info.Sample1Exception = info.Sample2Exception = ex;
                        info.Sample1ProcessFail = info.Sample2ProcessFail = true;
                    }
                    else if (ex.Channel == PipetteChannels.A)
                    {
                        info.Sample1Exception = ex;
                        info.Sample1ProcessFail = true;
                    }
                    else
                    {
                        info.Sample2Exception = ex;
                        info.Sample2ProcessFail = true;
                    }
                }
            });
        }
        protected virtual async Task UnAspirateAndUnloadTipsAsync(WorkunitSampleInfo info)
        {
            await Task.Run(() =>
            {
                if (info.Sample2 != null
                    && info.ContainerPos1.Row == info.ContainerPos2.Row
                    && (info.ContainerPos1.Column - info.ContainerPos2.Column) == -1
                    && !info.Sample1ProcessFail
                    && !info.Sample2ProcessFail)
                {
                    Alm.Pipettes.MoveTo(info.TargetDeepPlatePosition, info.ContainerPos1.Row, info.ContainerPos1.Column);
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
                        PipetteChannels channel = info.TargetDeepPlatePosition == PipettePositions.POS3 ? PipetteChannels.A : PipetteChannels.B;
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
        protected virtual async Task CoverAsync(WorkunitSampleInfo info, string tubeId)
        {
            await Task.Run(() =>
            {
                Alm.LidUncover.Cover(tubeId, info.AssignedUnit);
                Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos1, SliderSamplePosState.Occupied); //检查传感器
                if (info.Sample2 != null)
                {
                    Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos2, SliderSamplePosState.Occupied); //检查传感器
                }
            });
        }
        protected virtual async Task TransferToHotelAsync(WorkunitSampleInfo info, string tubeId)
        {
            await Task.Run(async () =>
            {
                Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos1, SliderSamplePosState.Occupied); //检查传感器
                Alm.Robot.GraspOnly(tubeId, info.SliderRobotPostion1);
                Alm.LidUncover.ReleaseCAxis(tubeId, info.AssignedUnit, LidUncoverCReleaseLevel.ForGrasp);
                Alm.Robot.GraspGoBack(info.SliderRobotPostion1);
                Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos1, SliderSamplePosState.Empty); //检查传感器
                Task tTightenCAxis = Task.Run(() =>
                {
                    if (info.Sample2 != null)
                    {
                        Alm.LidUncover.TightenCAxis(tubeId, info.AssignedUnit);
                    }
                });
                Alm.Robot.Loosen(tubeId, GetHotelRobotLocation(), info.Sample1.Position.Row, info.Sample1.Position.Column);
                if (info.Sample1ProcessFail)
                {
                    OnUnitError(new WorkflowUnitErrorEventArgs(CurrentPlateName, info.Sample1.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos1, info.Barcode1, info.Sample1Exception));
                }
                else
                {
                    OnUnitComplete(new WorkflowUnitCompleteEventArgs(CurrentPlateName, info.Sample1.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos1, info.Barcode1));
                }
                if (info.Sample2 != null)
                {
                    await tTightenCAxis;
                    Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos2, SliderSamplePosState.Occupied); //检查传感器
                    Alm.Robot.GraspOnly(tubeId, info.SliderRobotPostion2);
                    Alm.LidUncover.ReleaseCAxis(tubeId, info.AssignedUnit, LidUncoverCReleaseLevel.ForGrasp);
                    Alm.Robot.GraspGoBack(info.SliderRobotPostion2);
                    Alm.LidUncover.AssertSamplePosState(info.AssignedUnit, info.SliderPos2, SliderSamplePosState.Empty); //检查传感器
                    Alm.Robot.Loosen(tubeId, GetHotelRobotLocation(), info.Sample2.Position.Row, info.Sample2.Position.Column);
                    if (info.Sample2ProcessFail)
                    {
                        OnUnitError(new WorkflowUnitErrorEventArgs(CurrentPlateName, info.Sample2.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos2, info.Barcode2, info.Sample2Exception));
                    }
                    else
                    {
                        OnUnitComplete(new WorkflowUnitCompleteEventArgs(CurrentPlateName, info.Sample2.Position, DeepPlateName, info.TargetDeepPlatePosition, info.ContainerPos2, info.Barcode2));
                    }
                }
            });
        }
        protected virtual async Task ResetRAxisAsync(LidUncoverUnits workUnit)
        {
            await Task.Run(() =>
            {
                Alm.LidUncover.ResetRAxis(workUnit);
            });
        }
        protected virtual async Task ReleaseFailedGripperAsync(WorkunitSampleInfo info)
        {
            await Task.Run(() =>
            {
                Mgi.Gripper.Zimma.IZimmaGripper gripper;
                if (info.Sample1ProcessFail || info.Sample2ProcessFail)
                {
                    if (info.Sample1ProcessFail)
                    {
                        gripper = Alm.LidUncover.WorkUnits[info.AssignedUnit].GripperA;
                    }
                    else
                    {
                        gripper = Alm.LidUncover.WorkUnits[info.AssignedUnit].GripperB;
                    }
                    gripper.ReleaseGripper();
                }
            });
        }
        protected virtual async Task TightenGrippersAsync(string tubeId, LidUncoverUnits workUnit)
        {
            await Task.Run(() =>
            {
                Alm.LidUncover.TightenGrippers(tubeId, workUnit);
            });
        }
        protected virtual async Task UnLoadTipAsync()
        {
            await Task.Run(() =>
            {
                Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                Alm.Pipettes.UnloadTips();
            });
        }
    }
}
