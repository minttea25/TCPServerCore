using System;
using System.Collections.Generic;

namespace ServerCoreTCP.Utils
{

    // lock을 최소한으로 하거나, Concurrent Collection으로 사용할 수는 없을까...?

    /// <summary>
    /// PriorityQueue (If the type is numeric, Peek() will return highest value.)
    /// NOTE: If count==0, `Peek()` and `Dequeue()` will return DEFAULT value.
    /// Use `TryPeek` and `TryDequeue` safely with primitive types.
    /// </summary>
    /// <typeparam name="T">System.IComparable: the comparable object</typeparam>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        readonly List<T> _heap = new List<T>();
        public int Count => _heap.Count;
        public bool Empty => _heap.Count == 0;

        public void Enqueue(T item)
        {
            _heap.Add(item);

            int currentIdx = _heap.Count - 1;
            int nextIdx = (currentIdx - 1) / 2;

            while (currentIdx > 0 && _heap[currentIdx].CompareTo(_heap[nextIdx]) > 0)
            {
                Swap(currentIdx, nextIdx);

                currentIdx = nextIdx;
                nextIdx = (currentIdx - 1) / 2;
            }
        }

        public bool TryDequeue(out T peek)
        {
            if (_heap.Count == 0)
            {
                // Note
                // default(T) returns
                // null for reference types
                // 0 for value types
                peek = default;
                return false;
            }
            else
            {
                peek = Dequeue();
                return true;
            }
        }

        public T Dequeue()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("PriorityQueue is empty. Can not be dequeued.");
            }

            T result = _heap[0];
            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);

            int currentIdx = 0;

            while (true)
            {
                int left = currentIdx * 2 + 1;
                int right = currentIdx * 2 + 2;

                int smallest = currentIdx;

                if (left < _heap.Count && _heap[left].CompareTo(_heap[smallest]) > 0)
                {
                    smallest = left;
                }

                if (right < _heap.Count && _heap[right].CompareTo(_heap[smallest]) > 0)
                {
                    smallest = right;
                }

                if (smallest == currentIdx) break;

                Swap(currentIdx, smallest);
                currentIdx = smallest;
            }

            return result;
        }

        public T Peek()
        {
            return _heap.Count == 0 ? default : _heap[0];
        }

        public bool TryPeek(out T peek)
        {
            if (_heap.Count == 0)
            {
                peek = default;
                return false;
            }
            peek = _heap[0];
            return true;
        }

        public void Clear()
        {
            _heap.Clear();
        }

        void Swap(int idx1, int idx2)
        {
            T temp = _heap[idx1];
            _heap[idx1] = _heap[idx2];
            _heap[idx2] = temp;
        }
    }

    /// <summary>
    /// PrioirtyQueue with user-defined Compare function.
    /// </summary>
    /// <typeparam name="T">Any object</typeparam>
    public class PriorityQueueCompare<T>
    {
        readonly List<T> _heap = new List<T>();
        readonly Comparison<T> _compare;
        public int Count => _heap.Count;
        public bool Empty => _heap.Count == 0;

        /// <summary>
        /// PrioirtyQueue with user-defined Compare function.
        /// </summary>
        /// <param name="compare">The function to compare two objects.</param>
        public PriorityQueueCompare(Comparison<T> compare)
        {
            _compare = compare;
        }

        public void Enqueue(T item)
        {
            _heap.Add(item);

            int currentIdx = _heap.Count - 1;
            int nextIdx = (currentIdx - 1) / 2;

            while (currentIdx > 0 && _compare(_heap[currentIdx], _heap[nextIdx]) < 0)
            {
                Swap(currentIdx, nextIdx);

                currentIdx = nextIdx;
                nextIdx = (currentIdx - 1) / 2;
            }
        }

        public bool TryDequeue(out T peek)
        {
            if (_heap.Count == 0)
            {
                // Note
                // default(T) returns
                // null for reference types
                // 0 for value types
                peek = default;
                return false;
            }
            else
            {
                peek = Dequeue();
                return true;
            }
        }

        public T Dequeue()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("PriorityQueue is empty. Can not be dequeued.");
            }

            T result = _heap[0];
            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);

            int currentIdx = 0;

            while (true)
            {
                int left = currentIdx * 2 + 1;
                int right = currentIdx * 2 + 2;

                int smallest = currentIdx;

                if (left < _heap.Count && _compare(_heap[left], _heap[smallest]) < 0)
                {
                    smallest = left;
                }

                if (right < _heap.Count && _compare(_heap[right], _heap[smallest]) < 0)
                {
                    smallest = right;
                }

                if (smallest == currentIdx) break;

                Swap(currentIdx, smallest);
                currentIdx = smallest;
            }

            return result;
        }

        public T Peek()
        {
            return _heap.Count == 0 ? throw new InvalidOperationException("PrioirtyQueue is empty.") : _heap[0];
        }

        public void Clear()
        {
            _heap.Clear();
        }

        void Swap(int idx1, int idx2)
        {
            T temp = _heap[idx1];
            _heap[idx1] = _heap[idx2];
            _heap[idx2] = temp;
        }
    }

    /// <summary>
    /// PriorityQueue (If the type is numeric, Peek() will return highest value.)
    /// </summary>
    /// <typeparam name="T">System.IComparable: the comparable object</typeparam>
    public class ConcurrentPriorityQueue<T> where T : IComparable<T>
    {
        readonly List<T> _heap = new List<T>();
        readonly object _lock = new object();
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _heap.Count;
                }
            }
        }
        public bool Empty
        {
            get
            {
                lock (_lock)
                {
                    return _heap.Count == 0;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _heap.Add(item);

                int currentIdx = _heap.Count - 1;
                int nextIdx = (currentIdx - 1) / 2;

                while (currentIdx > 0 && _heap[currentIdx].CompareTo(_heap[nextIdx]) > 0)
                {
                    Swap(currentIdx, nextIdx);

                    currentIdx = nextIdx;
                    nextIdx = (currentIdx - 1) / 2;
                }
            }
        }

        public bool TryDequeue(out T peek)
        {
            lock (_lock)
            {
                if (_heap.Count == 0)
                {
                    // Note
                    // default(T) returns
                    // null for reference types
                    // 0 for value types
                    peek = default;
                    return false;
                }
                else
                {
                    peek = DequeueRaw();
                    return true;
                }
            }
        }

        public T Dequeue()
        {
            lock (_lock)
            {
                return _heap.Count == 0 ? throw new InvalidOperationException("PriorityQueue is empty. Can not be dequeued.") : DequeueRaw();
            }
        }

        public T Peek()
        {
            lock (_lock)
            {
                return _heap.Count == 0 ? throw new InvalidOperationException("PrioirtyQueue is empty.") : _heap[0];
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _heap.Clear();
            }
        }

        public void Clear(Action<T> action)
        {
            lock (_lock)
            {
                while (_heap.Count != 0)
                {
                    T e = DequeueRaw();
                    action?.Invoke(e);
                }
            }
        }

        T DequeueRaw()
        {
            T result = _heap[0];
            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);

            int currentIdx = 0;

            while (true)
            {
                int left = currentIdx * 2 + 1;
                int right = currentIdx * 2 + 2;

                int smallest = currentIdx;

                if (left < _heap.Count && _heap[left].CompareTo(_heap[smallest]) > 0)
                {
                    smallest = left;
                }

                if (right < _heap.Count && _heap[right].CompareTo(_heap[smallest]) > 0)
                {
                    smallest = right;
                }

                if (smallest == currentIdx) break;

                Swap(currentIdx, smallest);
                currentIdx = smallest;
            }

            return result;
        }

        void Swap(int idx1, int idx2)
        {
            T temp = _heap[idx1];
            _heap[idx1] = _heap[idx2];
            _heap[idx2] = temp;
        }
    }

    /// <summary>
    /// PrioirtyQueue with user-defined Compare function.
    /// </summary>
    /// <typeparam name="T">Any object</typeparam>
    public class ConcurrentPriorityQueueCompare<T>
    {
        readonly List<T> _heap = new List<T>();
        readonly object _lock = new object();
        readonly Comparison<T> _compare;
        public int Count {
            get
            {
                lock (_lock)
                {
                    return _heap.Count;
                }
            }
        }
        public bool Empty {
            get
            {
                lock (_lock)
                {
                    return _heap.Count == 0;
                }
            }
        }

        /// <summary>
        /// PrioirtyQueue with user-defined Compare function.
        /// </summary>
        /// <param name="compare">The function to compare two objects.</param>
        public ConcurrentPriorityQueueCompare(Comparison<T> compare)
        {
            _compare = compare;
        }

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _heap.Add(item);

                int currentIdx = _heap.Count - 1;
                int nextIdx = (currentIdx - 1) / 2;

                while (currentIdx > 0 && _compare(_heap[currentIdx], _heap[nextIdx]) < 0)
                {
                    Swap(currentIdx, nextIdx);

                    currentIdx = nextIdx;
                    nextIdx = (currentIdx - 1) / 2;
                }
            }
        }

        public bool TryDequeue(out T peek)
        {
            lock (_lock)
            {
                if (_heap.Count == 0)
                {
                    // Note
                    // default(T) returns
                    // null for reference types
                    // 0 for value types
                    peek = default;
                    return false;
                }
                else
                {
                    peek = DequeueRaw();
                    return true;
                }
            }
        }

        public T Dequeue()
        {
            lock (_lock)
            {
                return _heap.Count == 0 ? throw new InvalidOperationException("PriorityQueue is empty. Can not be dequeued.") : DequeueRaw();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _heap.Clear();
            }
        }

        public void Clear(Action<T> action)
        {
            lock (_lock)
            {
                while (_heap.Count != 0)
                {
                    T e = DequeueRaw();
                    action?.Invoke(e);
                }
            }
        }

        public T Peek()
        {
            return _heap.Count == 0 ? throw new InvalidOperationException("PrioirtyQueue is empty.") : _heap[0];
        }

        T DequeueRaw()
        {
            T result = _heap[0];
            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);

            int currentIdx = 0;

            while (true)
            {
                int left = currentIdx * 2 + 1;
                int right = currentIdx * 2 + 2;

                int smallest = currentIdx;

                if (left < _heap.Count && _compare(_heap[left], _heap[smallest]) < 0)
                {
                    smallest = left;
                }

                if (right < _heap.Count && _compare(_heap[right], _heap[smallest]) < 0)
                {
                    smallest = right;
                }

                if (smallest == currentIdx) break;

                Swap(currentIdx, smallest);
                currentIdx = smallest;
            }

            return result;
        }

        void Swap(int idx1, int idx2)
        {
            T temp = _heap[idx1];
            _heap[idx1] = _heap[idx2];
            _heap[idx2] = temp;
        }
    }
}


