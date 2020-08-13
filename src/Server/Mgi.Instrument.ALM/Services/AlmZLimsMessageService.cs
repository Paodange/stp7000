using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Mgi.ALM.ZLims;
using Mgi.ALM.ZLims.Protocol;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Services
{
    public class AlmZLimsMessageService : IZLimsMessageService
    {
        static readonly ILog log = Log4Manager.GetLogger("ZLims");
        public string DeviceId { get; }
        readonly IZLimsMessageHandler _messageHandler;
        readonly ConcurrentQueue<ZLimsMessage> innerQueque;
        volatile bool started = false;
        public AlmZLimsMessageService(ZLimsConfig config)
        {
            DeviceId = config.DeviceId;
            innerQueque = new ConcurrentQueue<ZLimsMessage>();
            _messageHandler = new KafkaZLimsMessageHandler(log, config);
        }

        public void Push(ZLimsMessage message)
        {
            innerQueque.Enqueue(message);
        }

        private ZLimsMessage Pop()
        {
            if (innerQueque.TryDequeue(out var message))
            {
                return message;
            }
            return null;
        }

        public void Start()
        {
            if (started) return;
            started = true;
            Task.Run(() =>
            {
                while (started)
                {
                    ZLimsMessage m = null;
                    while ((m = Pop()) != null)
                    {
                        _messageHandler.SendMessage(m);
                    }
                    Thread.Sleep(100);
                }
            });
        }

        public void Stop()
        {
            if (!started) return;
            started = false;
            Thread.Sleep(1000);
        }
    }
}
