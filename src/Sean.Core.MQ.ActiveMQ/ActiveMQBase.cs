using System;

namespace Sean.Core.MQ.ActiveMQ
{
    public abstract class ActiveMQBase
    {
        protected abstract bool OnException(Exception exception);

        protected virtual bool OnException(Exception exception, EventHandler<HandleExceptionEventArgs> eventHandler)
        {
            if (eventHandler == null)
                return false;
            var args = new HandleExceptionEventArgs(exception);
            eventHandler(this, args);
            return args.IsHandled;
        }
    }

    public abstract class ActiveMQBase<T> //: ActiveMQBase
    {
        protected ActiveMQOptions _mqOptions;

        protected abstract bool OnDataException(Exception exception, T data);

        protected virtual bool OnDataException(Exception exception, T data, EventHandler<HandleExceptionEventArgs<T>> eventHandler)
        {
            if (eventHandler == null)
                return false;
            var args = new HandleExceptionEventArgs<T>(exception, data,_mqOptions);
            eventHandler(this, args);
            return args.IsHandled;
        }
    }
}
