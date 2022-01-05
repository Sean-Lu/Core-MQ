using System;
using Demo.NetCore.Models;
using Microsoft.Extensions.Configuration;
using Sean.Core.MQ.ActiveMQ;
using Sean.Utility.Contracts;
using Sean.Utility.Format;

namespace Demo.NetCore.Consumers.ActiveMQ
{
    /// <summary>
    /// ActiveMQ
    /// </summary>
    public class TestConsumer : ISimpleService
    {
        //public static TestConsumer Instance { get; } = new TestConsumer();

        private readonly Consumer<TestModel> _consumer;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public TestConsumer(
            ISimpleLogger<TestConsumer> logger,
            IConfiguration configuration
            )
        {
            _logger = logger;//IocContainer.Instance.GetService<ISimpleLogger<TestConsumer>>();
            _configuration = configuration;//IocContainer.Instance.GetService<IConfiguration>();

            _consumer = new Consumer<TestModel>(_configuration, options => { options.Name = "Test"; }, ReceivedHandle);
        }

        public void Start()
        {
            _consumer.Start();
        }

        public void Stop()
        {
            _consumer.Stop();
        }

        private void ReceivedHandle(object sender, MessageReceivedEventArgs<TestModel> args)
        {
            try
            {
                var info = args.Data;
                if (info == null)
                {
                    return;
                }

                _logger.LogInfo($"消费者接收到消息：{JsonHelper.Serialize(info)}");
            }
            catch (Exception)
            {
                args.ShouldRecovery = true;// 重新消费消息

                // 对于异常处理的2种方式：
                // 1. 重抛异常：统一在 Consumer.DataExceptionListener 事件中处理
                // 2. 不抛异常：直接在这里处理，如记录异常日志等
                throw;
            }
        }
    }
}
