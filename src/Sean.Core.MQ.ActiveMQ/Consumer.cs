using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Newtonsoft.Json;
using Sean.Core.MQ.ActiveMQ.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 消费者
    /// </summary>
    public abstract class Consumer : ActiveMQBase<object>, IDisposable
    {
        /// <summary>
        /// 消费消息异常
        /// </summary>
        public static event EventHandler<HandleExceptionEventArgs<object>> DataExceptionListener;
        /// <summary>
        /// 连接异常
        /// </summary>
        public static event EventHandler<HandleExceptionEventArgs<IConnection>> ConnectionExceptionListener;

        public ConsumerOptions Options => _options;

        protected readonly ConsumerOptions _options;
        protected IConnection _connection;
        protected ISession _session;
        protected IMessageConsumer _consumer;

        protected CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private long _counter;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="options"></param>
        private Consumer(ConsumerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            Init();
        }

#if NETSTANDARD
        protected Consumer(IConfiguration configuration, Action<ConsumerOptions> config)
#else
        protected Consumer(Action<ConsumerOptions> config)
#endif
        {
#if NETSTANDARD
            _options = new ConsumerOptions(configuration);
#else
            _options = new ConsumerOptions();
#endif
            config?.Invoke(_options);

            Init();
        }

        /// <summary>
        /// 消费者开始
        /// </summary>
        public virtual void Start()
        {
            if (_connection != null)
            {
                if (!_connection.IsStarted)
                    _connection.Start();
                return;
            }

            _connection = ConnectionPool.Instance.GetConnection(_options);
            _connection.ExceptionListener += ex =>
            {
                ConnectionExceptionListener?.Invoke(this, new HandleExceptionEventArgs<IConnection>(ex, _connection, _options));
            };

            _session = _connection.CreateSession(_options.AcknowledgementMode);
            if (_options.Durable && _options.Type == MQType.Topic && !string.IsNullOrEmpty(_options.DurableClientId))
                _consumer = _session.CreateDurableConsumer(new ActiveMQTopic(_options.Name), _options.DurableClientId, _options.Selector, _options.Nolocal);//消息持久化
            else
                _consumer = _session.CreateConsumer(_options.Type.CreateDestination(_options.Name), _options.Selector, _options.Nolocal);
            _consumer.Listener += Consumer_Listener;

            ResetCancellationSource();

            _connection.Start();
        }

        /// <summary>
        /// 消费者停止
        /// </summary>
        /// <param name="secondsTimeout"></param>
        public virtual void Stop(int secondsTimeout = 15)
        {
            //取消事件监听
            _consumer.Listener -= Consumer_Listener;
            //触发协作取消机制
            _cancelSource.Cancel();
            //等待处理中的逻辑执行完成
            if (_counter > 0 && _resetEvent.WaitOne(secondsTimeout * 1000))
                Thread.Sleep(1000); //再等一下等待发回确认消息
            //关闭session，断开连接
            Dispose(true);
        }

        private void Init()
        {
            base._mqOptions = _options;
        }

        /// <summary>
        /// 消费者消息接收监听
        /// </summary>
        /// <param name="message"></param>
        private void Consumer_Listener(IMessage message)
        {
            CounterIncrement();
            object data = null;
            try
            {
                OnMessageReceived(message, out data);
            }
            catch (Exception ex)
            {
                if (!OnDataException(ex, data))
                    throw;
            }
            finally
            {
                CounterDecrement();
            }
        }

        protected abstract void OnMessageReceived(IMessage message, out object data);

        //protected override bool OnException(Exception exception)
        //{
        //    return base.OnException(exception, UnHandledException);
        //}
        protected override bool OnDataException(Exception exception, object data)
        {
            return base.OnDataException(exception, data, DataExceptionListener);
        }

        protected void CounterIncrement()
        {
            Interlocked.Increment(ref _counter);
        }

        protected void CounterDecrement()
        {
            Interlocked.Decrement(ref _counter);
            if (_cancelSource.IsCancellationRequested && _counter <= 0)
                _resetEvent.Set();
        }

        private void ResetCancellationSource()
        {
            _cancelSource?.Dispose();
            this._cancelSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 释放（默认释放连接）
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposeConnection">是否释放连接（考虑多消费者共用同一个连接的情况）</param>
        public void Dispose(bool disposeConnection)
        {
            _consumer?.Dispose();
            _session?.Dispose();

            if (disposeConnection)
            {
                ConnectionPool.Instance.DisposeConnection(_options);
                _connection = null;
            }
        }
    }

    /// <summary>
    /// 消费者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Consumer<T> : Consumer where T : class
    {
        //public static event EventHandler<HandleExceptionEventArgs<T>> DataExceptionListener;

        /// <summary>
        /// 接收消息的事件
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs<T>> MessageReceived;

#if NETSTANDARD
        public Consumer(IConfiguration configuration, Action<ConsumerOptions> config, EventHandler<MessageReceivedEventArgs<T>> messageReceived = null) : base(configuration, config)
#else
        public Consumer(Action<ConsumerOptions> config, EventHandler<MessageReceivedEventArgs<T>> messageReceived = null) : base(config)
#endif
        {
            this.MessageReceived += messageReceived;
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        protected virtual void OnMessageReceived(IMessage msg, T data)
        {
            var args = new MessageReceivedEventArgs<T>(data, _session, msg, _cancelSource.Token);
            try
            {
                MessageReceived?.Invoke(this, args);
            }
            finally
            {
                if (args.ShouldRecovery)
                {
                    //TODO: transaction support
                    _session.Recover();
                }
                else
                {
                    if (_session.AcknowledgementMode == AcknowledgementMode.ClientAcknowledge)
                    {
                        msg.Acknowledge();
                    }
                }
            }
        }

        protected override void OnMessageReceived(IMessage message, out object data)
        {
            var model = _options.Protocol.Get<T>(message);
            data = model;
            OnMessageReceived(message, model);
        }

        //protected override bool OnDataException(Exception exception, T data)
        //{
        //    return base.OnDataException(exception, data, DataExceptionListener);
        //}
    }
}
