namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 延迟和定时发送特性参数
    /// </summary>
    public class ActiveMQSchedule
    {
        /// <summary>
        /// The time in milliseconds that a message will wait before being scheduled to be delivered by the broker
        /// </summary>
        public long? Delay { get; set; }
        /// <summary>
        /// The time in milliseconds to wait after the start time to wait before scheduling the message again
        /// </summary>
        public long? Period { get; set; }
        /// <summary>
        /// The number of times to repeat scheduling a message for delivery
        /// </summary>
        public int? Repeat { get; set; }
        /// <summary>
        /// Use a Cron entry to set the schedule
        /// </summary>
        public string Cron { get; set; }
    }
}
