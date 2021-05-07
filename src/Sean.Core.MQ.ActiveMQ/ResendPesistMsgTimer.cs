using System.Linq;
using System.Timers;
using Apache.NMS;
using Newtonsoft.Json;
using Sean.Core.MQ.ActiveMQ.MsgStore;

namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 消息补偿定时器
    /// </summary>
    internal class ResendPesistMsgTimer : IStartable, IStoppable
    {
        public static ResendPesistMsgTimer Instance { get; } = new ResendPesistMsgTimer();

        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool IsStarted { get; private set; }

        private readonly Timer _timer;
        private bool _isRunning;

        private ResendPesistMsgTimer()
        {
            _timer = new Timer
            {
                AutoReset = true,
                Interval = 15 * 1000
            };
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            _timer.Start();

            IsStarted = true;
        }

        public void Stop()
        {
            if (!IsStarted)
            {
                return;
            }

            _timer.Stop();

            IsStarted = false;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;

            try
            {
                ResendPersistMsg(false);
            }
            finally
            {
                _isRunning = false;
            }
        }

        /// <summary>
        /// 重新发送持久化存储的消息
        /// </summary>
        /// <returns></returns>
        private void ResendPersistMsg(bool groupBy = false)
        {
            var list = MsgStoreHelper.Extract();
            if (list == null || list.Count <= 0)
                return;

            if (groupBy)
            {
                var listGroupBy = list.GroupBy(c => JsonConvert.SerializeObject(c.Options));
                foreach (var items in listGroupBy)
                {
                    var options = JsonConvert.DeserializeObject<ProducerOptions>(items.Key);
                    using (var producer = new Producer(options))
                    {
                        foreach (var model in items)
                        {
                            producer.Send(model);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in list)
                {
                    using (var producer = new Producer(item.Options))
                    {
                        producer.Send(item);
                    }
                }
            }
        }
    }
}
