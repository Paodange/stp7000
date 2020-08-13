using System;
using System.Collections.Generic;
using Mgi.Instrument.ALM.Workflow.Events;

namespace Mgi.Instrument.ALM.Workflow
{
    public interface IALMWorkflow
    {
        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="transferPlateInfos"></param>
        /// <param name="tipsContainer"></param>
        void Start(IEnumerable<TransferPlateInfo> transferPlateInfos, IEnumerable<MicroPlate> tipsContainer);
        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();
        /// <summary>
        /// 恢复继续
        /// </summary>
        void Resume();
        /// <summary>
        /// 停止
        /// </summary>
        void Stop();

        event EventHandler<EventArgs> WorkflowBegin;
        event EventHandler<EventArgs> WorkflowEnd;
        event EventHandler<WorkflowUnitEventArgs> UnitBegin;
        event EventHandler<WorkflowUnitCompleteEventArgs> UnitComplete;
        //event EventHandler<WorkflowBarcodeReadEventArgs> BarcodeRead;
        event EventHandler<WorkflowUnitErrorEventArgs> UnitError;
        event EventHandler<EventArgs> WorkflowPaused;
        event EventHandler<EventArgs> WorkflowResume;
        /// <summary>
        /// 排液完成事件
        /// </summary>
        event EventHandler<WorkflowUnitUnAspirateCompleteEventAgrs> UnitUnAspirateComplete;
        event EventHandler<WorkflowTransferPlateEventAgrs> PlateBegin;
        event EventHandler<WorkflowTransferPlateEventAgrs> PlateEnd;
    }


    public class TransferPlateInfo
    {
        /// <summary>
        /// 试管规格id
        /// </summary>
        public string TubeId { get; set; }
        /// <summary>
        /// 要转移的体积
        /// </summary>
        public double Volume { get; set; }
        /// <summary>
        /// 要转移的试管信息
        /// </summary>
        public MicroPlate Plate { get; set; }
        /// <summary>
        /// 深孔板信息
        /// </summary>
        public MicroPlate DeepPlateContainer { get; set; }

    }
}
