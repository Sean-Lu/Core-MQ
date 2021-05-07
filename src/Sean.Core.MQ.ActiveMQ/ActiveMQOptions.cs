using System.Configuration;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Policies;
using Newtonsoft.Json;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Sean.Core.MQ.ActiveMQ
{
    public class ActiveMQOptions : ConnectionOptions
    {
        /// <summary>
        /// 消息队列名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MQType Type { get; set; }
        /// <summary>
        /// 消息传输协议类型
        /// </summary>
        public TransferProtocol Protocol { get; set; }
        public AcknowledgementMode AcknowledgementMode { get; set; }

#if NETSTANDARD
        public ActiveMQOptions(IConfiguration configuration)
        {
            if (configuration != null)
            {
                BrokerUri = configuration.GetSection($"{Constants.ConfigurationRootName}:{Constants.ConfigurationBrokeUri}")?.Value;
                //NameSuffix = configuration.GetSection($"{Constants.ConfigurationRootName}:{Constants.ConfigurationNameSuffix}")?.Value;
            }
        }
#else
        public ActiveMQOptions()
        {
            BrokerUri = ConfigurationManager.AppSettings[$"{Constants.ConfigurationRootName}{Constants.ConfigurationBrokeUri}"];
            //NameSuffix = ConfigurationManager.AppSettings[$"{Constants.ConfigurationRootName}{Constants.ConfigurationNameSuffix}"];// 消息队列名称后缀（用于支持不同环境下消息队列名称可以不一致）
        }
#endif
    }

    public class ConnectionOptions
    {
        /// <summary>
        /// 消息队列地址
        /// </summary>
        public string BrokerUri { get; set; }
        public string ClientId { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public bool NonBlockingRedelivery { get; set; } = true;
        public RedeliveryPolicy RedeliveryPolicy { get; set; } = new RedeliveryPolicy { UseExponentialBackOff = true };
        public PrefetchPolicy PrefetchPolicy { get; set; }
    }

    public class ConsumerOptions : ActiveMQOptions
    {
        public bool Durable { get; set; }
        public string DurableClientId { get; set; }
        public string Selector { get; set; }
        public bool Nolocal { get; set; }

#if NETSTANDARD
        public ConsumerOptions(IConfiguration configuration) : base(configuration)
        {
        }
#endif
    }

    public class ProducerOptions : ActiveMQOptions
    {
        /// <summary>
        /// 消息传递模式
        /// </summary>
        public MsgDeliveryMode DefaultMsgDeliveryMode { get; set; }
        /// <summary>
        /// 消息级别，默认值：<see cref="MsgPriority.Normal"/>
        /// </summary>
        public MsgPriority DefaultMsgPriority { get; set; } = MsgPriority.Normal;

        /// <summary>
        /// 当出现异常时，是否持久化存储消息（默认保存路径：\ActiveMqData\*.data），默认值：true
        /// </summary>
        public bool PersistMsgWhenException { get; set; } = true;
        /// <summary>
        /// 是否对重新发送的持久化存储消息做备份保存（默认保存路径：\ActiveMqData\Resend\*.data），默认值：false
        /// </summary>
        public bool BackupResendPersistMsg { get; set; }

#if NETSTANDARD
        public ProducerOptions(IConfiguration configuration) : base(configuration)
        {
        }
#endif
    }
}
