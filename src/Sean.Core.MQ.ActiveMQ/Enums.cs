namespace Sean.Core.MQ.ActiveMQ
{
    public enum MQType
    {
        /// <summary>
        /// 点对点（point to point）
        /// </summary>
        Queue,
        /// <summary>
        /// 发布/订阅（publish/subscribe）
        /// </summary>
        Topic
    }

    public enum TransferProtocol
    {
        /// <summary>
        /// ITextMessage
        /// </summary>
        Json,
        /// <summary>
        /// IObjectMessage
        /// </summary>
        Binary
    }

    internal enum ScheduledMessage
    {
        /// <summary>
        /// 使用一个cron表达式来表示消息投递的策略
        /// </summary>
        AMQ_SCHEDULED_CRON,
        /// <summary>
        /// broker在投递该消息前等待的毫秒数
        /// </summary>
        AMQ_SCHEDULED_DELAY,
        /// <summary>
        /// 每次重新投递该消息的时间间隔
        /// </summary>
        AMQ_SCHEDULED_PERIOD,
        /// <summary>
        /// 重复投递该消息的次数
        /// </summary>
        AMQ_SCHEDULED_REPEAT
    }
}
