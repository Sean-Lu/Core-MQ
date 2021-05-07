using Demo.NetCore.Consumers;
using Demo.NetCore.Models;
using Microsoft.Extensions.Configuration;
using Sean.Core.Ioc;
using Sean.Core.MQ.ActiveMQ;
using Sean.Utility.Contracts;
using System;
using Apache.NMS;
using Sean.Utility.Format;

namespace Demo.NetCore.Impls.Test
{
    /// <summary>
    /// 消息队列
    /// </summary>
    public class ActiveMQTest : ISimpleDo
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public ActiveMQTest(
            ISimpleLogger<ActiveMQTest> logger,
            IConfiguration configuration
            )
        {
            _logger = logger;//ServiceManager.GetService<ISimpleLogger<ActiveMQTest>>();
            _configuration = configuration;//ServiceManager.GetService<IConfiguration>();

            Producer.ResendPesistMsgTimerInterval = 3 * 1000;
            Producer.StartResendPesistMsgTimer();// 手动启动消息补偿定时器

            #region 连接异常处理
            Consumer.ConnectionExceptionListener += (sender, args) =>
            {
                _logger.LogError("【消费者】连接异常", args.Exception);
            };
            Producer.ConnectionExceptionListener += (sender, args) =>
            {
                _logger.LogError("【生产者】连接异常", args.Exception);
            };
            #endregion

            #region 数据异常处理
            Consumer.DataExceptionListener += (sender, e) =>
            {
                if (e.Data is TestModel)
                {
                    _logger.LogError($"【消费者】消费消息异常{Environment.NewLine}数据：{JsonHelper.Serialize(e.Data)}{Environment.NewLine}配置：{JsonHelper.Serialize(e.Options)}", e.Exception);
                    e.IsHandled = true;// true:不会抛异常
                }
            };
            Producer.DataExceptionListener += (sender, e) =>
            {
                if (e.Data is TestModel)
                {
                    _logger.LogError($"【生产者】发送消息异常{Environment.NewLine}数据：{JsonHelper.Serialize(e.Data)}{Environment.NewLine}配置：{JsonHelper.Serialize(e.Options)}", e.Exception);
                    e.IsHandled = true;// true:不会抛异常
                }
            };
            #endregion

            //TestConsumer.Instance.Start();
            ServiceManager.GetService<TestConsumer>().Start();
        }

        public void Execute()
        {
            using (var producer = new Producer<TestModel>(_configuration, options =>
            {
                options.Name = "Test";
                options.DefaultMsgDeliveryMode = MsgDeliveryMode.NonPersistent;
                //options.PersistMsgWhenException = false;
            }))
            {
                var model = new TestModel
                {
                    Id = 10001,
                    Name = "Sean",
                    Age = 26,
                    Address = "中国",
                    CreateTime = DateTime.Now
                };
                producer.Send(model);
            }
        }
    }
}
