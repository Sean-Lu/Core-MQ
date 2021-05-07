using System;

namespace Sean.Core.MQ.ActiveMQ.MsgStore
{
    /// <summary>
    /// MQ消息存储
    /// </summary>
    internal class MsgStoreModel
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Guid { get; set; }

        public ProducerOptions Options { get; set; }

        /// <summary>
        /// 消息定时、延时发送
        /// </summary>
        public ActiveMQSchedule Schedule { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public object Msg { get; set; }
        /// <summary>
        /// 消息数据类型
        /// </summary>
        public Type MsgDataType { get; set; }
        /// <summary>
        /// 异常内容
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// 尝试重新发送消息的次数
        /// </summary>
        public int TryResendCount { get; set; }
    }
}
