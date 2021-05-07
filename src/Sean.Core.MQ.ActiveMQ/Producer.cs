using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Newtonsoft.Json;
using Sean.Core.MQ.ActiveMQ.Extensions;
using Sean.Core.MQ.ActiveMQ.MsgStore;
using System;
using System.Linq;

#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 生产者
    /// </summary>
    public class Producer : ActiveMQBase<object>, IDisposable
    {
        /// <summary>
        /// 发送消息异常
        /// </summary>
        public static event EventHandler<HandleExceptionEventArgs<object>> DataExceptionListener;
        /// <summary>
        /// 连接异常
        /// </summary>
        public static event EventHandler<HandleExceptionEventArgs<IConnection>> ConnectionExceptionListener;

        /// <summary>
        /// 消息补偿定时器触发的时间间隔（以毫秒为单位），默认值：15s
        /// </summary>
        public static double ResendPesistMsgTimerInterval
        {
            get => ResendPesistMsgTimer.Instance.Interval;
            set => ResendPesistMsgTimer.Instance.Interval = value;
        }

        public ProducerOptions Options => _options;

        private readonly ProducerOptions _options;
        private IConnection _connection;
        private IDestination _destination;

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <param name="options"></param>
        internal Producer(ProducerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            Init();
        }

#if NETSTANDARD
        public Producer(IConfiguration configuration, Action<ProducerOptions> config)
#else
        public Producer(Action<ProducerOptions> config)
#endif
        {
#if NETSTANDARD
            _options = new ProducerOptions(configuration);
#else
            _options = new ProducerOptions();
#endif
            config?.Invoke(_options);

            Init();
        }

        /// <summary>
        /// 手动启动消息补偿定时器（在每次发送消息时，会自动检测是否有持久化存储消息，如果有，也会自动开启）
        /// </summary>
        public static void StartResendPesistMsgTimer()
        {
            if (!ResendPesistMsgTimer.Instance.IsStarted)
            {
                ResendPesistMsgTimer.Instance.Start();
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="schedule"></param>
        public void Send(object msg, ActiveMQSchedule schedule = null)
        {
            Send(msg, schedule, ex =>
            {
                return new MsgStoreModel
                {
                    Guid = Guid.NewGuid().ToString().Replace("-", ""),
                    Options = _options,
                    Schedule = schedule,
                    Msg = msg,
                    MsgDataType = msg.GetType(),
                    Exception = ex?.Message
                };
            });

            if (!ResendPesistMsgTimer.Instance.IsStarted && MsgStoreHelper.ExistPersistMsg(out _))
            {
                ResendPesistMsgTimer.Instance.Start();
            }
        }

        /// <summary>
        /// 消息补偿（重发）
        /// </summary>
        /// <param name="model"></param>
        internal void Send(MsgStoreModel model)
        {
            Send(model.Msg, null, ex =>
            {
                model.TryResendCount++;
                model.Exception = ex?.Message;
                return model;
            });// 注：消息补偿不要设置定时或延时
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="schedule"></param>
        /// <param name="persistMsg"></param>
        private void Send(object msg, ActiveMQSchedule schedule, Func<Exception, MsgStoreModel> persistMsg)
        {
            if (msg == null)
            {
                return;
            }

            try
            {
                using (var session = _connection.CreateSession(_options.AcknowledgementMode))
                {
                    using (var producer = session.CreateProducer(_destination))
                    {
                        var message = _options.Protocol.CreateMessage(producer, msg);
                        if (schedule != null)
                        {
                            if (!string.IsNullOrWhiteSpace(schedule.Cron))
                                message.Properties.SetString(ScheduledMessage.AMQ_SCHEDULED_CRON.ToString(), schedule.Cron);
                            if (schedule.Delay.HasValue)
                                message.Properties.SetLong(ScheduledMessage.AMQ_SCHEDULED_DELAY.ToString(), schedule.Delay.Value);
                            if (schedule.Period.HasValue)
                                message.Properties.SetLong(ScheduledMessage.AMQ_SCHEDULED_PERIOD.ToString(), schedule.Period.Value);
                            if (schedule.Repeat.HasValue)
                                message.Properties.SetInt(ScheduledMessage.AMQ_SCHEDULED_REPEAT.ToString(), schedule.Repeat.Value);
                        }
                        //throw new Exception("测试异常。。。");
                        producer.Send(message, _options.DefaultMsgDeliveryMode, _options.DefaultMsgPriority, TimeSpan.MinValue);
                    }
                }
            }
            //catch (ConnectionClosedException)
            //{
            //    // The connection is already closed!
            //}
            catch (Exception ex)
            {
                if (_options.PersistMsgWhenException)
                {
                    // PersistMsg: 持久化存储消息
                    MsgStoreQueue.Enqueue(persistMsg?.Invoke(ex));
                }

                if (!OnDataException(ex, msg))
                    throw;
            }
        }

        private void Init()
        {
            base._mqOptions = _options;
            _connection = ConnectionPool.Instance.GetConnection(_options);
            _connection.ExceptionListener += ex =>
            {
                ConnectionExceptionListener?.Invoke(this, new HandleExceptionEventArgs<IConnection>(ex, _connection, _options));
            };
            _destination = _options.Type.CreateDestination(_options.Name);
        }

        //protected override bool OnException(Exception exception)
        //{
        //    return base.OnException(exception, UnHandledException);
        //}
        protected override bool OnDataException(Exception exception, object data)
        {
            return base.OnDataException(exception, data, DataExceptionListener);
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
        /// <param name="disposeConnection">是否释放连接（考虑多生产者共用同一个连接的情况）</param>
        public void Dispose(bool disposeConnection)
        {
            if (disposeConnection)
            {
                ConnectionPool.Instance.DisposeConnection(_options);
            }
        }
    }

    /// <summary>
    /// 生产者
    /// </summary>
    public class Producer<T> : Producer where T : class
    {
        //public static event EventHandler<HandleExceptionEventArgs<T>> DataExceptionListener;

#if NETSTANDARD
        public Producer(IConfiguration configuration, Action<ProducerOptions> config) : base(configuration, config)
#else
        public Producer(Action<ProducerOptions> config) : base(config)
#endif
        {
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="schedule"></param>
        public void Send(T msg, ActiveMQSchedule schedule = null)
        {
            base.Send(msg, schedule);
        }

        //protected override bool OnDataException(Exception exception, T data)
        //{
        //    return base.OnDataException(exception, data, DataExceptionListener);
        //}
    }
}
