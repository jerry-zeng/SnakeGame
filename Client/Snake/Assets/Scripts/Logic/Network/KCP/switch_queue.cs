//using System;
using System.Collections.Generic;

namespace Framework.Network.Kcp
{
    public class SwitchQueue<T> where T : class
    {
        private Queue<T> mConsumeQueue;
        private Queue<T> mProduceQueue;

        public SwitchQueue()
        {
            mConsumeQueue = new Queue<T>(16);
            mProduceQueue = new Queue<T>(16);
        }

        public SwitchQueue(int capcity)
        {
            mConsumeQueue = new Queue<T>(capcity);
            mProduceQueue = new Queue<T>(capcity);
        }

        // producer
        public void Push(T obj)
        {
            lock (mProduceQueue)
            {
                mProduceQueue.Enqueue(obj);
            }
        }

        // consumer.
        public T Pop()
        {
            return mConsumeQueue.Dequeue();
        }

        public bool Empty()
        {
            return mConsumeQueue.Count == 0;
        }

        public void Switch()
        {
            lock(mProduceQueue)
            {
                Swap(ref mConsumeQueue, ref mProduceQueue);
            }
        }

        public void Clear()
        {
            lock(mProduceQueue)
            {
                mConsumeQueue.Clear();
                mProduceQueue.Clear();
            }
        }


        public static void Swap<QT>(ref QT t1, ref QT t2)
        {
            QT temp = t1;
            t1 = t2;
            t2 = temp;
        }
    }

}
