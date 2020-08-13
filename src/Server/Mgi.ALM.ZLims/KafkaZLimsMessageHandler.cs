using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Mgi.ALM.ZLims.Protocol;
using Newtonsoft.Json;

namespace Mgi.ALM.ZLims
{
    public class KafkaZLimsMessageHandler : IZLimsMessageHandler
    {
        private readonly ZLimsConfig _config;
        private IProducer<Null, string> producer;
        private IConsumer<Ignore, string> consumer;
        public KafkaZLimsMessageHandler(ZLimsConfig config)
        {
            //_log = log;
            _config = config;
            InitProducer(config);
            InitConsumer(config);
            //_log.Info($"kafka config:Address:{config.Servers},GroupId:{config.GroupId},Retry:{config.RetryTimes}");
        }


        private void InitProducer(ZLimsConfig config)
        {
            var pc = new ProducerConfig()
            {
                Debug = "Topic",
                BootstrapServers = config.Servers,
                ClientId = config.DeviceId,

            };
            ProducerBuilder<Null, string> builder = new ProducerBuilder<Null, string>(pc);
            producer = builder.Build();
        }

        private void InitConsumer(ZLimsConfig config)
        {
            var cConfig = new ConsumerConfig
            {
                BootstrapServers = config.Servers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                ClientId = config.DeviceId,
                Debug = "Topic",
                GroupId = config.GroupId,
            };
            consumer?.Close();
            consumer?.Dispose();
            consumer = null;

            consumer = new ConsumerBuilder<Ignore, string>(cConfig).Build();
        }

        public void SendMessage(ZLimsMessage message)
        {
            for (int i = 0; i < _config.RetryTimes; i++)
            {
                try
                {
                    //_log.Info(message?.ToJson());
                    //producer.Produce(message.MessageGroup, new Message<Null, string>() { Value = message.ToJson() });
                    producer.ProduceAsync(message.MessageGroup, new Message<Null, string>() { Value = message.ToJson() })
                               .ContinueWith(t => HandleProduceFailure(message.MessageId, t));
                    //_log.Info($"[{message?.MessageId}] The message has been sent");
                    break;
                }
                catch (Exception ex)
                {
                    if (i == _config.RetryTimes - 1)
                    {
                        //_log.Error($"Send message to zlims fail,message:{message.ToJson()}", ex);
                    }
                }
            }
        }

        private void HandleProduceFailure(string key, Task<DeliveryResult<Null, string>> task)
        {
            try
            {
                var delivery = task.Result;
                if (task.IsFaulted)
                {
                    //_log.Error($"[{key}] | Send Faulted, {delivery.Offset}, {delivery.Topic}, {delivery.Value}", task.Exception);
                }
                else
                {
                    //_log.Info($"[{key}] | Send Success, {delivery.Offset}, {delivery.Topic}");
                }
            }
            catch (Exception ex)
            {
                //_log.Error($"[{key}] | {ex}");
            }
        }

        public async Task SendMessageAsync(ZLimsMessage message)
        {
            for (int i = 0; i < _config.RetryTimes; i++)
            {
                try
                {
                    await producer.ProduceAsync(message.MessageGroup, new Message<Null, string>() { Value = JsonConvert.SerializeObject(message) });
                    break;
                }
                catch (Exception ex)
                {
                    if (i == _config.RetryTimes - 1)
                    {
                        //_log.Error($"Send message to zlims fail,message:{message.ToJson()}", ex);
                    }
                }
            }

        }
    }
}
