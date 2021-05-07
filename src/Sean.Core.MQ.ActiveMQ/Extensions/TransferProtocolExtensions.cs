using System;
using Apache.NMS;
using Newtonsoft.Json;

namespace Sean.Core.MQ.ActiveMQ.Extensions
{
    public static class TransferProtocolExtensions
    {
        public static IMessage CreateMessage(this TransferProtocol protocol, IMessageProducer messageProducer, object msg)
        {
            switch (protocol)
            {
                case TransferProtocol.Binary:
                    return messageProducer.CreateObjectMessage(msg);
                case TransferProtocol.Json:
                    return messageProducer.CreateTextMessage(msg is string s ? s : JsonConvert.SerializeObject(msg));
                default:
                    throw new NotSupportedException($"{typeof(TransferProtocol).FullName}.{protocol.ToString()}");
            }
        }

        public static T Get<T>(this TransferProtocol protocol, IMessage msg) where T : class
        {
            switch (protocol)
            {
                case TransferProtocol.Binary:
                    if (msg is IObjectMessage obj)
                    {
                        return obj.Body as T;
                    }
                    break;
                case TransferProtocol.Json:
                    if (msg is ITextMessage text)
                    {
                        return typeof(T) == typeof(string) ? text.Text as T : JsonConvert.DeserializeObject<T>(text.Text);
                    }
                    break;
                default:
                    throw new NotSupportedException($"{typeof(TransferProtocol).FullName}.{protocol.ToString()}");
            }

            return default;
        }
    }
}
