using System;
using System.Collections.Generic;

namespace BubbleShot.Core
{
    /// <summary>
    /// Lightweight, garbage-free generic object pool for high-frequency game objects,
    /// particle instances, and floating text callouts.
    /// Pure C# with zero engine dependencies.
    /// </summary>
    public class ObjectPool<T>
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T>? _onGet;
        private readonly Action<T>? _onReturn;
        private readonly int _maxCapacity;

        public int CountInactive => _pool.Count;
        public int CountActive { get; private set; }

        public ObjectPool(
            Func<T> createFunc,
            Action<T>? onGet = null,
            Action<T>? onReturn = null,
            int initialCapacity = 16,
            int maxCapacity = 128)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onReturn = onReturn;
            _maxCapacity = maxCapacity;
            _pool = new Stack<T>(initialCapacity);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count && _pool.Count < _maxCapacity; i++)
            {
                var item = _createFunc();
                _onReturn?.Invoke(item);
                _pool.Push(item);
            }
        }

        public T Rent()
        {
            T item;
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                item = _createFunc();
            }

            CountActive++;
            _onGet?.Invoke(item);
            return item;
        }

        public void Return(T item)
        {
            if (item == null) return;

            CountActive = Math.Max(0, CountActive - 1);
            _onReturn?.Invoke(item);

            if (_pool.Count < _maxCapacity)
            {
                _pool.Push(item);
            }
        }

        public void Clear()
        {
            _pool.Clear();
            CountActive = 0;
        }
    }
}
