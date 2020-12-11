using System.Collections.Generic;

namespace GSFC.ARVR.UTILITIES
{
    public class FixedSizeQueue<T>
    {
        private Queue<T> q = new Queue<T>();
        public int limit = 8;

        private object lockObject = new object();

        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            lock (lockObject)
            {
                while (q.Count > limit)
                {
                    q.Dequeue();
                }
            }
        }

        public T Dequeue()
        {
            return q.Dequeue();
        }

        public int GetCount()
        {
            return q.Count;
        }
    }
}