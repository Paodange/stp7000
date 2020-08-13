using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Util;
using Mgi.Instrument.ALM.Workflow.Events;
using Mgi.Instrument.ALM.Workflow;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mgi.Instrument.ALM.Workflow
{
    /// <summary>
    /// 串行流程运行引擎
    /// </summary>
    public class SerializableWorkflow : AbstractALMWorkflow
    {
        public SerializableWorkflow(IAutoLidMachine _alm)
            : base(_alm)
        {
        }

        protected override void Run(string tubeId, double volume)
        {
            PlateStorageUnit sample;
            while ((sample = GetNextSample()) != null)
            {
                AutoResetEvent mreRobot1 = new AutoResetEvent(true);     // 线程1机械臂占用锁
                AutoResetEvent mrePipette1 = new AutoResetEvent(true);   // 线程1 移液器占用锁

                AutoResetEvent mreRobot2 = new AutoResetEvent(true);      // 线程2机械臂占用锁
                AutoResetEvent mrePipette2 = new AutoResetEvent(true);    // 线程2移液器占用锁

                PlatePosition containerPos1 = default(PlatePosition), containerPos2 = default(PlatePosition), containerPos3 = default(PlatePosition), containerPos4 = default(PlatePosition);
                PlateStorageUnit sample2 = null, sample3 = null, sample4 = null;
                containerPos1 = GetNextContainerPosition();
                Alm.LidUncover.ReleaseCAxis(tubeId, LidUncoverUnits.A, LidUncoverCReleaseLevel.ForLoosen);
                Alm.LidUncover.SliderMoveTo(LidUncoverUnits.A, SliderPositionEnum.RobotPosition);
                OnUnitBegin(new WorkflowUnitEventArgs(CurrentPlateName, sample.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos1));
                Alm.Robot.Grasp(tubeId, GetHotelRobotLocation(), sample.Position.Row, sample.Position.Column);
                Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS1, SliderSamplePosState.Empty);
                Alm.Robot.Loosen(tubeId, RobotLocation.ASlider1);
                Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS1, SliderSamplePosState.Occupied); //检查传感器
                sample2 = GetNextSample();
                if (sample2 != null)
                {
                    containerPos2 = GetNextContainerPosition();
                    OnUnitBegin(new WorkflowUnitEventArgs(CurrentPlateName, sample2.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos2));
                    Alm.Robot.Grasp(tubeId, GetHotelRobotLocation(), sample2.Position.Row, sample2.Position.Column);
                    Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS2, SliderSamplePosState.Empty);
                    Alm.Robot.Loosen(tubeId, RobotLocation.ASlider2);
                    Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS2, SliderSamplePosState.Occupied); //检查传感器
                }
                Alm.LidUncover.TightenCAxis(tubeId, LidUncoverUnits.A);
                Alm.LidUncover.SliderMoveTo(LidUncoverUnits.A, SliderPositionEnum.LidUncover);


                Task t2 = Task.Run(() =>
                  {
                      sample3 = GetNextSample();
                      if (sample3 != null)
                      {
                          Alm.LidUncover.ReleaseCAxis(tubeId, LidUncoverUnits.B, LidUncoverCReleaseLevel.ForLoosen);
                          Alm.LidUncover.SliderMoveTo(LidUncoverUnits.B, SliderPositionEnum.RobotPosition);
                          containerPos3 = GetNextContainerPosition();
                          base.OnUnitBegin(new WorkflowUnitEventArgs(CurrentPlateName, sample3.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos3));
                          Alm.Robot.Grasp(tubeId, GetHotelRobotLocation(), sample3.Position.Row, sample3.Position.Column);
                          Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS1, SliderSamplePosState.Empty);
                          Alm.Robot.Loosen(tubeId, RobotLocation.BSlider1);
                          Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS1, SliderSamplePosState.Occupied); //检查传感器
                          sample4 = GetNextSample();
                          if (sample4 != null)
                          {
                              containerPos4 = GetNextContainerPosition();
                              base.OnUnitBegin(new WorkflowUnitEventArgs(CurrentPlateName, sample4.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos4));
                              Alm.Robot.Grasp(tubeId, GetHotelRobotLocation(), sample4.Position.Row, sample4.Position.Column);
                              Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS2, SliderSamplePosState.Empty);
                              Alm.Robot.Loosen(tubeId, RobotLocation.BSlider2);
                              Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS2, SliderSamplePosState.Occupied); //检查传感器
                          }
                          mreRobot2.Set();
                          Alm.LidUncover.TightenCAxis(tubeId, LidUncoverUnits.B);
                          Alm.LidUncover.SliderMoveTo(LidUncoverUnits.B, SliderPositionEnum.LidUncover);
                          var barcodeGroup2 = Alm.LidUncover.UnCoverAndScan(tubeId, LidUncoverUnits.B, sample4 == null ? LidUncoverGripper.A : LidUncoverGripper.Both); //拔盖
                          if (barcodeGroup2 == null || barcodeGroup2.Count() != 2)
                          {
                              // 报错用户点击忽略可能会返回空 如果为空 则没有条码  否则后续拿条码会报错
                              barcodeGroup2 = new string[2];
                          }
                          Alm.LidUncover.SliderMoveTo(LidUncoverUnits.B, SliderPositionEnum.Pipettes); //
                                                                                                       // 等待前两个样本移液完成
                          mrePipette1.WaitOne();
                          if (sample3 != null)
                          {
                              if (sample4 != null)
                              {
                                  // 需要两个通道同时移液
                                  Alm.Pipettes.MoveTo(PipettePositions.POS1, containerPos3.Row, containerPos3.Column);
                                  Alm.Pipettes.LoadTips(PipettePositions.POS1);
                                  Alm.Pipettes.MoveTo(PipettePositions.POS6, 1, 1);
                                  Alm.Pipettes.AspirateBoth(tubeId, PipettePositions.POS6, volume);
                                  Alm.Pipettes.MoveTo(GetTargetDeepPlatePostion(), containerPos3.Row, containerPos3.Column);
                                  Alm.Pipettes.UnAspirateBoth(GetTargetDeepPlatePostion());
                                  Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                                  Alm.Pipettes.UnloadTips();
                              }
                              else
                              {
                                  // 单通道移液
                                  Alm.Pipettes.MoveTo(PipettePositions.POS1, containerPos3.Row, containerPos3.Column);
                                  Alm.Pipettes.LoadTip(PipetteChannels.A, PipettePositions.POS1);
                                  Alm.Pipettes.MoveTo(PipettePositions.POS6, 1, 1);
                                  Alm.Pipettes.Aspirate(tubeId, PipetteChannels.A, PipettePositions.POS6, volume);
                                  Alm.Pipettes.MoveTo(GetTargetDeepPlatePostion(), containerPos3.Row, containerPos3.Column);
                                  Alm.Pipettes.UnAspirate(PipetteChannels.A, GetTargetDeepPlatePostion());
                                  Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                                  Alm.Pipettes.UnLoadTip(PipetteChannels.A);
                              }
                          }
                          mrePipette2.Set();

                          Alm.LidUncover.SliderMoveTo(LidUncoverUnits.B, SliderPositionEnum.LidUncover);
                          Alm.LidUncover.Cover(tubeId, LidUncoverUnits.B);
                          Alm.LidUncover.SliderMoveTo(LidUncoverUnits.B, SliderPositionEnum.RobotPosition);
                          // 等待第1，2个样本使用机械臂完成下料  
                          mreRobot1.WaitOne();
                          // 第3，4个样本开始下料
                          Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS1, SliderSamplePosState.Occupied); //检查传感器
                          Alm.Robot.GraspOnly(tubeId, RobotLocation.BSlider1);
                          Alm.LidUncover.ReleaseCAxis(tubeId, LidUncoverUnits.B, LidUncoverCReleaseLevel.ForGrasp);
                          Alm.Robot.GraspGoBack(RobotLocation.BSlider1);
                          Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS1, SliderSamplePosState.Empty); //检查传感器
                          Alm.Robot.Loosen(tubeId, GetHotelRobotLocation(), sample3.Position.Row, sample3.Position.Column);
                          OnUnitComplete(new WorkflowUnitCompleteEventArgs(CurrentPlateName, sample3.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos3, barcodeGroup2.ElementAt(0)));
                          if (sample4 != null)
                          {
                              Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS2, SliderSamplePosState.Occupied); //检查传感器
                              Alm.Robot.GraspOnly(tubeId, RobotLocation.BSlider2);
                              Alm.LidUncover.ReleaseCAxis(tubeId, LidUncoverUnits.B, LidUncoverCReleaseLevel.ForGrasp);
                              Alm.Robot.GraspGoBack(RobotLocation.BSlider2);
                              Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.B, SliderSamplePos.POS2, SliderSamplePosState.Empty); //检查传感器
                              Alm.Robot.Loosen(tubeId, GetHotelRobotLocation(), sample4.Position.Row, sample4.Position.Column);
                              OnUnitComplete(new WorkflowUnitCompleteEventArgs(CurrentPlateName, sample4.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos4, barcodeGroup2.ElementAt(1)));
                          }
                          mreRobot2.Set();
                      }
                  });

                var barcodeGroup1 = Alm.LidUncover.UnCoverAndScan(tubeId, LidUncoverUnits.A, sample2 == null ? LidUncoverGripper.A : LidUncoverGripper.Both); //拔盖
                if (barcodeGroup1 == null || barcodeGroup1.Count() != 2)
                {
                    barcodeGroup1 = new string[2];
                }
                Alm.LidUncover.SliderMoveTo(LidUncoverUnits.A, SliderPositionEnum.Pipettes); //

                #region 移液部分
                mrePipette2.WaitOne();
                if (sample2 != null)
                {
                    // 需要两个通道同时移液
                    Alm.Pipettes.MoveTo(PipettePositions.POS1, containerPos1.Row, containerPos1.Column);
                    Alm.Pipettes.LoadTips(PipettePositions.POS1);
                    Alm.Pipettes.MoveTo(PipettePositions.POS5, 1, 1);
                    Alm.Pipettes.AspirateBoth(tubeId, PipettePositions.POS5, volume);
                    Alm.Pipettes.MoveTo(GetTargetDeepPlatePostion(), containerPos1.Row, containerPos1.Column);
                    Alm.Pipettes.UnAspirateBoth(GetTargetDeepPlatePostion());
                    Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                    Alm.Pipettes.UnloadTips();
                }
                else
                {
                    // 单通道移液
                    Alm.Pipettes.MoveTo(PipettePositions.POS1, containerPos1.Row, containerPos1.Column);
                    Alm.Pipettes.LoadTip(PipetteChannels.A, PipettePositions.POS1);
                    Alm.Pipettes.MoveTo(PipettePositions.POS5, 1, 1);
                    Alm.Pipettes.Aspirate(tubeId, PipetteChannels.A, PipettePositions.POS5, volume);
                    Alm.Pipettes.MoveTo(GetTargetDeepPlatePostion(), containerPos1.Row, containerPos1.Column);
                    Alm.Pipettes.UnAspirate(PipetteChannels.A, GetTargetDeepPlatePostion());
                    Alm.Pipettes.MoveTo(PipettePositions.POS7, 1, 1);
                    Alm.Pipettes.UnLoadTip(PipetteChannels.A);
                }
                // 第3，4个样本可以使用移液器移液
                mrePipette1.Set();
                #endregion


                Alm.LidUncover.SliderMoveTo(LidUncoverUnits.A, SliderPositionEnum.LidUncover);
                Alm.LidUncover.Cover(tubeId, LidUncoverUnits.A);
                Alm.LidUncover.SliderMoveTo(LidUncoverUnits.A, SliderPositionEnum.RobotPosition);
                mreRobot2.WaitOne();
                Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS1, SliderSamplePosState.Occupied); //检查传感器
                Alm.Robot.GraspOnly(tubeId, RobotLocation.ASlider1);
                Alm.LidUncover.ReleaseCAxis(tubeId, LidUncoverUnits.A, LidUncoverCReleaseLevel.ForGrasp);
                Alm.Robot.GraspGoBack(RobotLocation.ASlider1);
                Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS1, SliderSamplePosState.Empty); //检查传感器
                Alm.Robot.Loosen(tubeId, GetHotelRobotLocation(), sample.Position.Row, sample.Position.Column);
                OnUnitComplete(new WorkflowUnitCompleteEventArgs(CurrentPlateName, sample.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos1, barcodeGroup1.ElementAt(0)));
                if (sample2 != null)
                {
                    Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS1, SliderSamplePosState.Occupied); //检查传感器
                    Alm.Robot.GraspOnly(tubeId, RobotLocation.ASlider2);
                    Alm.LidUncover.ReleaseCAxis(tubeId, LidUncoverUnits.A, LidUncoverCReleaseLevel.ForGrasp);
                    Alm.Robot.GraspGoBack(RobotLocation.ASlider2);
                    Alm.LidUncover.AssertSamplePosState(LidUncoverUnits.A, SliderSamplePos.POS2, SliderSamplePosState.Empty); //检查传感器
                    Alm.Robot.Loosen(tubeId, GetHotelRobotLocation(), sample2.Position.Row, sample2.Position.Column);
                    OnUnitComplete(new WorkflowUnitCompleteEventArgs(CurrentPlateName, sample2.Position, DeepPlateName, GetTargetDeepPlatePostion(), containerPos2, barcodeGroup1.ElementAt(1)));
                }
                // 第3，4 个样本可以使用机械臂下料
                mreRobot1.Set();
                // 等待第3，4个样本完成
                try
                {
                    Task.WaitAll(t2);
                }
                catch (AggregateException e) when (e.InnerException is UserAbortException)
                {
                    throw e.InnerException;
                }
            }
        }
    }
}
