using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace Sean.Core.MQ.ActiveMQ.Extensions
{
    public static class MQTypeExtensions
    {
        public static IDestination CreateDestination(this MQType type, string name)
        {
            switch (type)
            {
                case MQType.Queue:
                    return new ActiveMQQueue(name);
                case MQType.Topic:
                    return new ActiveMQTopic(name);
                default:
                    throw new NotSupportedException($"{typeof(MQType).FullName}.{type.ToString()}");
            }
        }
    }
}
