using System;
using System.Collections.Generic;
using IronPython.Runtime;
using Mgi.ALM.ZLims.Protocol;
using Mgi.Instrument.ALM.Device;

namespace Mgi.Instrument.ALM.Util
{
    public class ZLimsMessageFactory
    {
        public string DeviceId { get; }

        public ZLimsMessageFactory(string deviceId)
        {
            DeviceId = deviceId;
        }

        public ZLimsMessage CreateRegisterMessage()
        {
            return new DeviceMessage<DeviceRegisterContent>()
            {
                MessageType = MessageTypeConstant.DEVICE_REGISTER,
                MessageContent = new DeviceRegisterContent()
                {
                    Components = new List<DeviceComponent>(),
                    DeviceDesc = "MGISTP-7000",
                    DeviceId = DeviceId,
                    DeviceType = "MGISTP-7000"
                }
            };
        }
        public ZLimsMessage CreateHeartbeatMessage(string status)
        {
            return new DeviceMessage<DeviceStatusContent>()
            {
                MessageType = MessageTypeConstant.DEVICE_HEARTBEAT,
                MessageContent = new DeviceStatusContent()
                {
                    DeviceId = DeviceId,
                    Status = status,
                    StatusTime = DateTime.Now,
                    Components = new List<ComponentStatus>()
                }
            };
        }
        public ZLimsMessage CreateStatusMessage(string status)
        {
            return new DeviceMessage<DeviceStatusContent>()
            {
                MessageType = MessageTypeConstant.DEVICE_STATUS,
                MessageContent = new DeviceStatusContent()
                {
                    DeviceId = DeviceId,
                    Components = new List<ComponentStatus>(),
                    Status = status,
                    StatusTime = DateTime.Now
                }
            };
        }

        public ZLimsMessage CreateDeviceLogMessage(string taskId, string code, string message)
        {
            return new DeviceMessage<DeviceLogContent>()
            {
                MessageType = MessageTypeConstant.DEVICE_LOG,
                MessageContent = new DeviceLogContent()
                {
                    TaskId = taskId,
                    LogTime = DateTime.Now,
                    DeviceId = DeviceId,
                    Handing = "",
                    LogCode = code,
                    LogDesc = message,
                    LogLevel = "Error",
                    LogType = "Error",
                }
            };
        }

        public ZLimsMessage CreateTaskPrepMessage(string taskId, List<Layout> layouts)
        {
            return new TaskMessage<TaskPrepareContent>()
            {
                MessageType = MessageTypeConstant.TASK_PREP,
                MessageContent = new TaskPrepareContent()
                {
                    TaskId = taskId,
                    PrepTime = DateTime.Now,
                    DeviceId = DeviceId,
                    Layouts = layouts
                }
            };
        }

        public ZLimsMessage CreateTaskStartMessage(string taskId)
        {
            return new TaskMessage<TaskStartContent>()
            {
                MessageType = MessageTypeConstant.TASK_START,
                MessageContent = new TaskStartContent()
                {
                    TaskId = taskId,
                    StartTime = DateTime.Now,
                    DeviceId = DeviceId,
                }
            };
        }

        public ZLimsMessage CreateTaskUpdateMessage(string taskId, UpdateOutput output)
        {
            return new TaskMessage<TaskUpdateContent>()
            {
                MessageType = MessageTypeConstant.TASK_UPDATE,
                MessageContent = new TaskUpdateContent()
                {
                    TaskId = taskId,
                    UpdateTime = DateTime.Now,
                    DeviceId = DeviceId,
                    Outputs = new List<UpdateOutput>()
                     {
                        output
                     },
                }
            };
        }

        public ZLimsMessage CreateTaskCompleteMessage(string taskId, bool completeStatus, List<CompleteOutput> outputs)
        {
            return new TaskMessage<TaskCompleteContent>()
            {
                MessageType = MessageTypeConstant.TASK_COMPLETE,
                MessageContent = new TaskCompleteContent()
                {
                    TaskId = taskId,
                    CompleteStatus = completeStatus,
                    DeviceId = DeviceId,
                    Outputs = outputs,
                    CompleteTime = DateTime.Now
                }
            };
        }
    }
}
