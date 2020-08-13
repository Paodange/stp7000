using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using Mgi.ALM.Util.Extension;
using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Util;
using Mgi.Instrument.ALM.Workflow.Events;

namespace Mgi.Instrument.ALM.Workflow
{
    public abstract class AbstractALMWorkflow : IALMWorkflow
    {
        protected static readonly ILog log = Log4Manager.GetLogger("Workflow");
        private volatile bool isPaused = false;
        protected readonly object syncLock = new object();
        private int sampleSkipCount = 0;
        private int sampleProcessIndex = 0;
        protected int SampleCount { get; set; }

        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
            set
            {
                if (isPaused != value)
                {
                    isPaused = value;
                    if (isPaused)
                    {
                        OnWorkflowPaused(new EventArgs());
                    }
                    else
                    {
                        OnWorkflowResume(new EventArgs());
                    }
                }
            }
        }
        /// <summary>
        /// 流程管理器  为流程运行提供暂停 恢复  停止操作
        /// </summary>
        protected IWorkflowManager WorkflowManager { get; }
        /// <summary>
        /// 流程运行前触发
        /// </summary>
        public event EventHandler<EventArgs> WorkflowBegin;
        /// <summary>
        /// 流程结束时触发
        /// </summary>
        public event EventHandler<EventArgs> WorkflowEnd;
        /// <summary>
        /// 单个样本开始处理前触发
        /// </summary>
        public event EventHandler<WorkflowUnitEventArgs> UnitBegin;
        /// <summary>
        /// 单个样本处理完成触发
        /// </summary>
        public event EventHandler<WorkflowUnitCompleteEventArgs> UnitComplete;
        /// <summary>
        /// 单个样本处理出错时触发
        /// </summary>
        public event EventHandler<WorkflowUnitErrorEventArgs> UnitError;
        /// <summary>
        /// 排液完成事件
        /// </summary>
        public event EventHandler<WorkflowUnitUnAspirateCompleteEventAgrs> UnitUnAspirateComplete;

        /// <summary>
        /// 流程暂停时触发
        /// </summary>
        public event EventHandler<EventArgs> WorkflowPaused;
        /// <summary>
        /// 流程继续时触发
        /// </summary>
        public event EventHandler<EventArgs> WorkflowResume;
        public event EventHandler<WorkflowTransferPlateEventAgrs> PlateBegin;
        public event EventHandler<WorkflowTransferPlateEventAgrs> PlateEnd;

        /// <summary>
        /// 获取孔板名称与目标深孔板位置的映射关系
        /// </summary>
        private static readonly Dictionary<string, PipettePositions> platePosMap
            = new Dictionary<string, PipettePositions>()
        {
            {"Plate1", PipettePositions.POS3 },
            {"Plate2", PipettePositions.POS4 },
        };
        /// <summary>
        /// 获取孔板名称与目标TIP盒位置的映射关系
        /// </summary>
        private static readonly Dictionary<string, PipettePositions> plateTipsPosMap
            = new Dictionary<string, PipettePositions>()
        {
            {"Tips1", PipettePositions.POS1 },
            {"Tips2", PipettePositions.POS2 },
        };
        /// <summary>
        /// 获取孔板名称与机械臂位置的映射关系
        /// </summary>
        private static readonly Dictionary<string, RobotLocation> plateHotelLocationMap
            = new Dictionary<string, RobotLocation>()
        {
            {"Plate1", RobotLocation.HotelA },
            {"Plate2", RobotLocation.HotelB },
        };
        /// <summary>
        /// 获取本次流程运行的样本容器
        /// </summary>
        protected Dictionary<string, MicroPlate> SampleContainers { get; private set; }
        protected Dictionary<string, MicroPlate> TipsContainers { get; private set; }
        /// <summary>
        /// 获取本次流程运行的所有样本信息
        /// </summary>
        protected IEnumerable<PlateStorageUnit> Samples { get; private set; }
        protected IAutoLidMachine Alm { get; }

        public AbstractALMWorkflow(IAutoLidMachine alm)
        {
            Alm = alm;
            WorkflowManager = alm.WorkflowManager;
            foreach (var inteceptor in GeneralInterceptor.AllFilters)
            {
                if (inteceptor is DeviceCommandInterceptor d)
                {
                    d.OnPauseComplete += (o, e) =>
                    {
                        IsPaused = true;
                    };
                    d.OnResumeComplete += (o, e) =>
                    {
                        IsPaused = false;
                    };
                    d.OnRetry += (o, e) => WorkflowManager.SetStatus(WorkflowStatus.Running);
                    d.OnIgnore += (o, e) => WorkflowManager.SetStatus(WorkflowStatus.Running);
                }
            }
            Alm.IOBoard.DoorStateChanged += IOBoard_DoorStateChanged;
        }
        private void IOBoard_DoorStateChanged(object sender, DoorStateChangedEventArgs e)
        {
            if (e.State == DoorState.Open && WorkflowManager.Status == WorkflowStatus.Running)
            {
                Pause();
            }
        }
        /// <summary>
        /// 获取当前正在处理的孔板名称
        /// </summary>
        public string CurrentPlateName { get; private set; }
        public string DeepPlateName { get; private set; }
        /// <summary>
        /// 开始流程
        /// </summary>
        /// <param name="transferPlateInfos"></param>
        /// <param name="tipsContainer"></param>
        public void Start(IEnumerable<TransferPlateInfo> transferPlateInfos, IEnumerable<MicroPlate> tipsContainer)
        {
            ResetMicroPlate(transferPlateInfos.Select(x => x.DeepPlateContainer), tipsContainer);
            useWorkUnitA = false;
            SampleContainers = new Dictionary<string, MicroPlate>();
            TipsContainers = tipsContainer.ToDictionary(x => x.Name);
            WorkflowManager.SetStatus(WorkflowStatus.Running);
            sampleProcessIndex = 0;
            SampleCount = transferPlateInfos.SelectMany(x => x.Plate.Positions).Where(x => x.Value != null && x.Value?.IsUsed == true).Count();
            OnWorkflowBegin(new EventArgs());
            try
            {
                Alm.IOBoard.Run();
                foreach (var item in transferPlateInfos)
                {
                    try
                    {
                        sampleSkipCount = 0;
                        CurrentPlateName = item.Plate.Name;
                        DeepPlateName = item.DeepPlateContainer.Name;
                        SampleContainers.Add(item.Plate.Name, item.DeepPlateContainer);
                        //TipsContainers.Add(item.Plate.Name, item.TipsContainer);
                        //SampleContainers.Add(item.Plate.Name, new MicroPlate("DeepPlate1", item.Plate.RowCount, item.Plate.ColumnCount));
                        Samples = item.Plate.Positions.Where(x => x.Value != null && x.Value?.IsUsed == true).Select(x => x.Value).OrderBy(x => x.Position.Row).ThenBy(x => x.Position.Column);
                        OnPlateBegin(new WorkflowTransferPlateEventAgrs(item));
                        Run(item.TubeId, item.Volume);
                    }
                    finally
                    {
                        OnPlateEnd(new WorkflowTransferPlateEventAgrs(item));
                    }
                }
            }
            catch (UserAbortException)
            {
                //
            }
            catch (AggregateException e) when (e.InnerException is UserAbortException)
            {
                //
            }
            finally
            {
                ResetMicroPlate(transferPlateInfos.Select(x => x.DeepPlateContainer), tipsContainer);
                Alm.IOBoard.Idle();
                WorkflowManager.SetStatus(WorkflowStatus.Stopped);
                OnWorkflowEnd(new EventArgs());
            }
        }
        /// <summary>
        /// 暂停流程
        /// </summary>
        public void Pause()
        {
            WorkflowManager.SetStatus(WorkflowStatus.Paused);
            DateTime d = DateTime.Now;
            while (!isPaused)
            {
                Thread.Sleep(100);
                if ((DateTime.Now - d).TotalSeconds > 3)
                {
                    isPaused = true;
                }
            }
        }
        /// <summary>
        /// 恢复流程
        /// </summary>
        public void Resume()
        {
            WorkflowManager.SetStatus(WorkflowStatus.Running);
            DateTime d = DateTime.Now;
            while (isPaused)
            {
                Thread.Sleep(100);
                if ((DateTime.Now - d).TotalSeconds > 3)
                {
                    isPaused = false;
                }
            }
        }
        /// <summary>
        /// 停止流程
        /// </summary>
        public virtual void Stop()
        {
            WorkflowManager.SetStatus(WorkflowStatus.Stopped);
        }
        /// <summary>
        /// 流程运行逻辑
        /// </summary>
        protected abstract void Run(string tubeId, double volume);
        protected virtual void OnWorkflowBegin(EventArgs e)
        {
            log.Info($"Workflow begin,Total:{SampleCount}");
            WorkflowBegin?.Invoke(this, e);
        }
        protected virtual void OnWorkflowEnd(EventArgs e)
        {
            log.Info($"Workflow end, Total:{SampleCount}");
            WorkflowEnd?.Invoke(this, e);
        }
        protected virtual void OnUnitBegin(WorkflowUnitEventArgs e)
        {
            Interlocked.Increment(ref sampleProcessIndex);
            log.Info($"Unit begin, Process Index:{sampleProcessIndex}/{SampleCount},info:{e.ToJsonString()}");
            UnitBegin?.Invoke(this, e);
        }
        protected virtual void OnUnitComplete(WorkflowUnitCompleteEventArgs e)
        {
            log.Info($"Unit end, Process Index:{sampleProcessIndex}/{SampleCount},info:{e.ToJsonString()}");
            UnitComplete?.Invoke(this, e);
        }
        protected virtual void OnUnitError(WorkflowUnitErrorEventArgs e)
        {
            log.Info($"Unit error, Process Index:{sampleProcessIndex}/{SampleCount},info:{e.ToJsonString()}");
            UnitError?.Invoke(this, e);
        }
        protected virtual void OnWorkflowPaused(EventArgs e)
        {
            log.Info("work flow paused");
            WorkflowPaused?.Invoke(this, e);
        }
        protected virtual void OnWorkflowResume(EventArgs e)
        {
            log.Info("work flow resume");
            WorkflowResume?.Invoke(this, e);
        }
        protected virtual void OnLoadTipComplete(TipInfo tipInfo)
        {
            TipsContainers[tipInfo.PlateName].SetMaterial(tipInfo.Position, new PlateStorageUnit()
            {
                IsUsed = true,
                Position = tipInfo.Position
            });
        }
        protected virtual void OnUnitUnAspirateComplete(WorkflowUnitUnAspirateCompleteEventAgrs e)
        {
            SampleContainers[CurrentPlateName].SetMaterial(e.TargetPosition, new PlateStorageUnit()
            {
                Barcode = e.Barcode,
                IsUsed = true,
                Position = e.TargetPosition,
                BarcodeReadFail = e.BarcodeReadFail
            });
            UnitUnAspirateComplete?.Invoke(this, e);
        }

        protected virtual void OnPlateBegin(WorkflowTransferPlateEventAgrs e)
        {
            log.Info($"Plate:{e.TransferPlateInfo.Plate.Name} begin,Target :{e.TransferPlateInfo.DeepPlateContainer.Name},Tube:{e.TransferPlateInfo.TubeId},Volume:{e.TransferPlateInfo.Volume},Samples:{Samples.ToJsonString()}");
            PlateBegin?.Invoke(this, e);
        }
        protected virtual void OnPlateEnd(WorkflowTransferPlateEventAgrs e)
        {
            log.Info($"Plate:{e.TransferPlateInfo.Plate.Name} end,Target :{e.TransferPlateInfo.DeepPlateContainer.Name},Tube:{e.TransferPlateInfo.TubeId},Volume:{e.TransferPlateInfo.Volume}");
            PlateEnd?.Invoke(this, e);
        }
        /// <summary>
        /// 获取下一个要抓取的样本
        /// </summary>
        /// <returns></returns>
        protected virtual PlateStorageUnit GetNextSample()
        {
            lock (syncLock)
            {
                log.Info($"GetNextSample Skip:{sampleSkipCount}");
                var result = Samples.ElementAtOrDefault(sampleSkipCount);
                sampleSkipCount++;
                return result;
            }
        }

        /// <summary>
        /// 获取下两个要处理的样本  和对应的目标位置
        /// </summary>
        /// <returns></returns>
        protected virtual WorkunitSampleInfo GetNextTwoSampleInfo()
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
                    pos1 = GetNextContainerPosition();
                    tipInfo1 = GetNextTipInfo();
                    if (sample2 != null)
                    {
                        pos2 = GetNextContainerPosition();
                        tipInfo2 = GetNextTipInfo();
                    }
                    var unit = GetWorkUnit();
                    var sliderPositions = GetSliderPositions(unit);
                    return new WorkunitSampleInfo()
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
                }
                return null;
            }
        }
        /// <summary>
        /// 获取下一个要存液的位置 (行，列)
        /// </summary>
        /// <returns></returns>
        protected virtual PlatePosition GetNextContainerPosition()
        {
            lock (syncLock)
            {
                var pos = SampleContainers[CurrentPlateName].GetNextEmptyPosition();
                if (pos == null)
                {
                    throw new Exception("Container Empty position not enough");
                }
                return pos.Value;
            }
        }

        /// <summary>
        /// 获取下一个可用的tip头位置
        /// </summary>
        /// <returns></returns>
        protected TipInfo GetNextTipInfo()
        {
            lock (syncLock)
            {
                foreach (var item in TipsContainers)
                {
                    var pos = item.Value.GetNextEmptyPosition();
                    if (pos != null)
                    {
                        return new TipInfo()
                        {
                            PlateName = item.Key,
                            Pos = plateTipsPosMap[item.Key],
                            Position = pos.Value
                        };
                    }
                }
                throw new Exception("Tips not enough");
            }
        }
        /// <summary>
        /// 获取当前孔板 要移动移液器的目标位置
        /// </summary>
        /// <returns></returns>
        protected PipettePositions GetTargetDeepPlatePostion()
        {
            return platePosMap[CurrentPlateName];
        }

        /// <summary>
        /// 获取当前孔板 对应的Hotel 机械臂位置
        /// </summary>
        /// <returns></returns>
        protected RobotLocation GetHotelRobotLocation()
        {
            return plateHotelLocationMap[CurrentPlateName];
        }
        /// <summary>
        /// 按序分配工作单元给每一个要处理的样本
        /// </summary>
        /// <returns></returns>
        protected LidUncoverUnits GetWorkUnit()
        {
            lock (syncLock)
            {
                useWorkUnitA = !useWorkUnitA;
                return useWorkUnitA ? LidUncoverUnits.A : LidUncoverUnits.B;
            }
        }

        protected Tuple<RobotLocation, RobotLocation> GetSliderPositions(LidUncoverUnits unit)
        {
            if (unit == LidUncoverUnits.A)
            {
                return new Tuple<RobotLocation, RobotLocation>(RobotLocation.ASlider1, RobotLocation.ASlider2);
            }
            else
            {
                return new Tuple<RobotLocation, RobotLocation>(RobotLocation.BSlider1, RobotLocation.BSlider2);
            }
        }

        private void ResetMicroPlate(IEnumerable<MicroPlate> deepPlates, IEnumerable<MicroPlate> tips)
        {
            foreach (var item in deepPlates)
            {
                item.ResetUsed();
            }
            foreach (var item in tips)
            {
                item.ResetUsed();
            }
        }

        private bool useWorkUnitA = false;
    }

    public class WorkunitSampleInfo
    {
        /// <summary>
        /// 样本1
        /// </summary>
        public PlateStorageUnit Sample1 { get; set; }
        /// <summary>
        /// 样本2
        /// </summary>
        public PlateStorageUnit Sample2 { get; set; }
        /// <summary>
        /// 要转移到的移液器目标POS
        /// </summary>
        public PipettePositions TargetSamplePosition { get; set; }
        /// <summary>
        /// 目标深孔板位置
        /// </summary>
        public PipettePositions TargetDeepPlatePosition { get; set; }
        /// <summary>
        /// 要转移样本1到的移液器目标POS的具体位置
        /// </summary>
        public PlatePosition ContainerPos1 { get; set; }
        /// <summary>
        /// 要转移样本2到的移液器目标POS的具体位置
        /// </summary>
        public PlatePosition ContainerPos2 { get; set; }
        public TipInfo TipInfo1 { get; set; }
        public TipInfo TipInfo2 { get; set; }
        /// <summary>
        /// 要使用的拔盖器工作单元
        /// </summary>
        public LidUncoverUnits AssignedUnit { get; set; }
        /// <summary>
        /// 样本1对应机械臂的抓取放下位点
        /// </summary>
        public RobotLocation SliderRobotPostion1 { get; set; }
        /// <summary>
        /// 样本2对应机械臂的抓取放下位点
        /// </summary>
        public RobotLocation SliderRobotPostion2 { get; set; }

        /// <summary>
        /// 第一个样本的滑块位置
        /// </summary>
        public SliderSamplePos SliderPos1 { get; set; }
        /// <summary>
        /// 第二个样本的滑块位置
        /// </summary>
        public SliderSamplePos SliderPos2 { get; set; }

        /// <summary>
        /// 样本1的条码
        /// </summary>
        public string Barcode1 { get; set; }
        /// <summary>
        /// 样本2条码
        /// </summary>
        public string Barcode2 { get; set; }

        public bool Sample1ProcessFail { get; set; } = false;
        public bool Sample2ProcessFail { get; set; } = false;
        public Exception Sample1Exception { get; set; }
        public Exception Sample2Exception { get; set; }

        /// <summary>
        /// 交换容器的位置和条码
        /// </summary>
        public void SwipeContainerPosAndBarcode()
        {
            var tempBarcode = Barcode1;
            Barcode1 = Barcode2;
            Barcode2 = tempBarcode;

            var tempPos = ContainerPos1;
            ContainerPos1 = ContainerPos2;
            ContainerPos2 = tempPos;
        }
    }

    public class TipInfo
    {
        public string PlateName { get; set; }
        public PipettePositions Pos { get; set; }
        public PlatePosition Position { get; set; }
    }
}
