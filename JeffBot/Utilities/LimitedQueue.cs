using System.Collections.Generic;

namespace JeffBot
{
    public class LimitedQueue<T> : Queue<T>
    {
        private readonly int _capacity;

        #region Constructor
        public LimitedQueue(int capacity)
        {
            this._capacity = capacity;
        }
        #endregion

        #region Enqueue
        public void LimitedEnqueue(T item)
        {
            if (Count == _capacity)
            {
                Dequeue();
            }
            Enqueue(item);
        }
        #endregion
    }
}