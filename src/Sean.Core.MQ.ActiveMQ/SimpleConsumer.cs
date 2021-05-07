using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Policies;
using Sean.Core.MQ.ActiveMQ.Extensions;

namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 消费者
    /// </summary>
    public class SimpleConsumer
    {
        private readonly Action<ITextMessage> _messageReceived;
        private readonly Func<Exception, bool> _onException;

        private readonly IConnectionFactory _connectionFactory;
        private readonly IDestination _destination;

        private IConnection _connection;
        private ISession _session;
        private IMessageConsumer _msgconsumer;

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="brokerUri">消息队列地址</param>
        /// <param name="type">消息队列类型</param>
        /// <param name="name">消息队列名称</param>
        /// <param name="messageReceived">接收消息</param>
        /// <param name="onException">异常处理，返回true则会重新消费</param>
        public SimpleConsumer(string brokerUri, MQType type, string name, Action<ITextMessage> messageReceived, Func<Exception, bool> onException = null)
        {
            if (string.IsNullOrWhiteSpace(brokerUri))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(brokerUri));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            _messageReceived = messageReceived;
            _onException = onException;

            _connectionFactory = new ConnectionFactory(brokerUri);
            _destination = type.CreateDestination(name);
        }

        /// <summary>
        /// 开始
        /// </summary>
        public virtual void Start()
        {
            if (_connection != null)
            {
                if (!_connection.IsStarted)
                    _connection.Start();
                return;
            }

            _connection = _connectionFactory.CreateConnection();
            _connection.RedeliveryPolicy = new RedeliveryPolicy { UseExponentialBackOff = true };
            _session = _connection.CreateSession();
            _msgconsumer = _session.CreateConsumer(_destination, null);
            _msgconsumer.Listener += Consumer_Listener;

            _connection.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public virtual void Stop()
        {
            if (_msgconsumer != null)
            {
                _msgconsumer.Listener -= Consumer_Listener;
                _msgconsumer.Dispose();
            }
            _session?.Dispose();
            _connection?.Dispose();
            _connection = null;
        }

        private void Consumer_Listener(IMessage message)
        {
            try
            {
                _messageReceived?.Invoke(message as ITextMessage);
            }
            catch (Exception ex)
            {
                if (_onException != null && _onException(ex))
                {
                    _session.Recover();// 重新消费
                }
            };
        }
    }
}
