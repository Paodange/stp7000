using System;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims.Protocol
{
    public abstract class ZLimsMessage
    {
        static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            Formatting = Formatting.Indented
        };
        [JsonProperty("message_id", Order = 10)]
        public string MessageId { get; set; }
        [JsonProperty("message_type", Order = 20)]
        public string MessageType { get; set; }
        [JsonProperty("message_group", Order = 30)]
        public string MessageGroup { get; protected set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, jsonSerializerSettings);
        }
    }
    public abstract class ZLimsMessage<T> : ZLimsMessage where T : class, new()
    {

        [JsonProperty("message_content", Order = 100)]
        public T MessageContent { get; set; }

        public ZLimsMessage()
        {
            MessageId = Guid.NewGuid().ToString("N");
        }
    }

    public class DeviceMessage<T> : ZLimsMessage<T> where T : class, new()
    {
        public DeviceMessage()
        {
            MessageGroup = MessageGroupConstant.DEVICE;
        }
    }

    public class TaskMessage<T> : ZLimsMessage<T> where T : class, new()
    {
        public TaskMessage()
        {
            MessageGroup = MessageGroupConstant.TASK;
        }
    }

    public class MessageTypeConstant
    {
        public const string DEVICE_REGISTER = "device_register";
        public const string DEVICE_HEARTBEAT = "device_heartbeat";
        public const string DEVICE_STATUS = "device_status";
        public const string DEVICE_LOG = "device_log";
        public const string TASK_PREP = "task_prep";
        public const string TASK_START = "task_start";
        public const string TASK_UPDATE = "task_update";
        public const string TASK_COMPLETE = "task_complete";
    }

    public class MessageGroupConstant
    {
        public const string DEVICE = "device";
        public const string TASK = "task";
    }
}
