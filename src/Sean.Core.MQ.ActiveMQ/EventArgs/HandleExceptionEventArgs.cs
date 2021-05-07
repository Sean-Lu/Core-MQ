using System;

namespace Sean.Core.MQ.ActiveMQ
{
    /// <summary>
    /// 处理异常的事件参数
    /// </summary>
    public class HandleExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// 异常是否已处理
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// 处理异常的事件参数
        /// </summary>
        /// <param name="ex"></param>
        public HandleExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }

    public class HandleExceptionEventArgs<T> : HandleExceptionEventArgs
    {
        public T Data { get; set; }
        public ActiveMQOptions Options { get; set; }

        public HandleExceptionEventArgs(Exception ex, T data, ActiveMQOptions options) : base(ex)
        {
            Data = data;
            Options = options;
        }
    }
}