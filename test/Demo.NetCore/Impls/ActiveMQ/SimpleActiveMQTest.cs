using Demo.NetCore.Consumers.ActiveMQ;
using Microsoft.Extensions.Configuration;
using Sean.Core.MQ.ActiveMQ;
using Sean.Utility.Contracts;

namespace Demo.NetCore.Impls.ActiveMQ
{
    /// <summary>
    /// ActiveMQ
    /// </summary>
    public class SimpleActiveMQTest : ISimpleDo
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly SimpleConsumer _consumer;
        private readonly SimpleProducer _producer;

        public SimpleActiveMQTest(
            ISimpleLogger<TestConsumer> logger,
            IConfiguration configuration
            )
        {
            _logger = logger;//IocContainer.Instance.GetService<ISimpleLogger<SimpleActiveMQTest>>();
            _configuration = configuration;//IocContainer.Instance.GetService<IConfiguration>();

            #region 消费者
            _consumer = new SimpleConsumer(new ActiveMQOptions(_configuration).BrokerUri, MQType.Queue, "Test_PromotionRecharge",
                message =>
                {
                    _logger.LogInfo($"消费者接收到消息：{message.Text}");
                    //throw new Exception("异常测试。。。");
                },
                ex =>
                {
                    _logger.LogError("【消费者】接收消息异常", ex);
                    return true;
                });
            _consumer.Start();
            #endregion

            #region 生产者
            _producer = new SimpleProducer(new ActiveMQOptions(_configuration).BrokerUri, MQType.Queue, "Test_PromotionRecharge",
                ex =>
                {
                    _logger.LogError("【生产者】发送消息异常", ex);
                });
            #endregion
        }

        public void Execute()
        {
            _producer.Send("{\"PayCash\":0.01,\"ProductId\":\"5d11c7d0ca25080001c2b931\",\"PromotionAwardCount\":0,\"PromotionCount\":2000,\"SupplierId\":101235,\"TradeNo\":\"2019062822001408370545110248\"}");
        }
    }
}
