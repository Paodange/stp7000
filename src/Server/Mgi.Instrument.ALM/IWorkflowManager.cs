using System;

namespace Mgi.Instrument.ALM
{
    /// <summary>
    /// 流程状态管理  用于控制设备的暂停，恢复，停止
    /// </summary>
    public interface IWorkflowManager
    {
        /// <summary>
        /// 
        /// </summary>
        WorkflowStatus Status { get; }

        void SetStatus(WorkflowStatus status);
    }

    public enum WorkflowStatus
    {
        None = 1,
        Running,
        Paused,
        Stopped
    }

    public class WorkflowManager : IWorkflowManager
    {
        private volatile WorkflowStatus status;
        public WorkflowStatus Status
        {
            get
            {
                return status;
            }
        }

        public event EventHandler OnPause;
        public event EventHandler OnResume;
        public event EventHandler OnStop;

        public void SetStatus(WorkflowStatus status)
        {
            if (this.status != status)
            {
                this.status = status;
                if (status == WorkflowStatus.Paused)
                {
                    OnPause?.Invoke(this, EventArgs.Empty);
                }
                else if (status == WorkflowStatus.Running)
                {
                    OnResume?.Invoke(this, EventArgs.Empty);
                }
                else if (status == WorkflowStatus.Stopped)
                {
                    OnStop?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
