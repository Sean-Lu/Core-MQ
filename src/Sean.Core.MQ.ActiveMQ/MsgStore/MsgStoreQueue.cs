using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sean.Core.MQ.ActiveMQ.MsgStore
{
    /// <summary>
    /// 消息体队列
    /// </summary>
    internal class MsgStoreQueue
    {
        private static ConcurrentQueue<MsgStoreModel> _queue = new ConcurrentQueue<MsgStoreModel>();

        public static void Enqueue(params MsgStoreModel[] items)
        {
            if (items == null)
                return;
            foreach (var item in items)
            {
                if (item == null)
                    continue;
                _queue.Enqueue(item);
            }
            Flush(_queue.Count);
        }

        private static void Flush(int size)
        {
            var items = new List<MsgStoreModel>();
            while (size-- > 0 && _queue.TryDequeue(out var item))
            {
                if (item == null)
                    continue;
                items.Add(item);
            }

            MsgStoreHelper.Persist(items);
        }
    }
}
