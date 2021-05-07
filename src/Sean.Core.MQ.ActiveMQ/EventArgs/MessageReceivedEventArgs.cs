using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 消息接收的事件参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageReceivedEventArgs<T> : EventArgs where T : class
    {
        /// <summary>
        /// 消息接收事件的参数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="session"></param>
        /// <param name="originmsg">原始消息</param>
        /// <param name="cancellationToken"></param>
        public MessageReceivedEventArgs(T data, ISession session, IMessage originmsg, CancellationToken cancellationToken)
        {
            this.Data = data;
            this.Session = session;
            this.OriginMsg = originmsg;
            this.CancellationToken = cancellationToken;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; }
        /// <summary>
        /// 是否需要恢复消息
        /// </summary>
        public bool ShouldRecovery { get; set; }
        /// <summary>
        /// 重试了多少次数
        /// </summary>
        public int RecoveryCount => (this.OriginMsg as Message)?.RedeliveryCounter ?? 0;
        /// <summary>
        /// Session
        /// </summary>
        public ISession Session { get; }
        /// <summary>
        /// 原始消息
        /// </summary>
        public IMessage OriginMsg { get; }
        /// <summary>
        /// CancellationToken
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}