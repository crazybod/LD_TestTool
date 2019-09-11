using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    public class QueueThreadSafe<T> : Queue<T>
    {
        public new void Enqueue(T obj)
        {
            lock (this)
            {
                base.Enqueue(obj);
            }
        }

        public new T Dequeue()
        {
            lock (this)
            {
                if (base.Count > 0)
                {
                    return base.Dequeue();
                }
                else
                {
                    return default(T);
                }
            }
        }

        public List<T> DequeueAll()
        {
            List<T>  list = new List<T>();
            lock (this)
            {
                 
                while (this.Count > 0)
                {
                    list.Add(base.Dequeue());
                }
                
            }
            return list;
        }

        public new void Clear()
        {
            lock (this)
            {
                base.Clear();
            }
        }
    }
}
