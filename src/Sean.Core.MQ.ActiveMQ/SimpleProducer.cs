using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Newtonsoft.Json;
using Sean.Core.MQ.ActiveMQ.Extensions;

namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 生产者
    /// </summary>
    public class SimpleProducer
    {
        private readonly string _brokerUri;
        private readonly string _name;
        private readonly IDestination _destination;
        private readonly Action<Exception> _onException;

        /// <summary>
        /// 创建消息生产者
        /// </summary>
        /// <param name="brokerUri">消息队列地址</param>
        /// <param name="type">消息队列类型</param>
        /// <param name="name">消息队列名称</param>
        /// <param name="onException">异常处理</param>
        public SimpleProducer(string brokerUri, MQType type, string name, Action<Exception> onException = null)
        {
            if (string.IsNullOrWhiteSpace(brokerUri))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(brokerUri));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            _brokerUri = brokerUri;
            _name = name;
            _destination = type.CreateDestination(name);
            _onException = onException;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        public void Send<T>(T msg)
        {
            if (msg == null)
            {
                return;
            }

            try
            {
                IConnectionFactory factory = new ConnectionFactory(_brokerUri);
                using (IConnection connection = factory.CreateConnection())
                {
                    using (ISession session = connection.CreateSession())
                    {
                        using (IMessageProducer messageProducer = session.CreateProducer(_destination))
                        {
                            messageProducer.Send(TransferProtocol.Json.CreateMessage(messageProducer, msg), MsgDeliveryMode.NonPersistent, MsgPriority.Normal, TimeSpan.MinValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }
        }
    }
}
