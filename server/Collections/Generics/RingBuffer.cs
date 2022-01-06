using System;

namespace OptimeGBAServer.Collections.Generics
{
    public class RingBuffer<T>
    {
        private readonly T[] buffer;
        private int current;

        public int Count { get; private set; }

        public RingBuffer(int capacity)
        {
            buffer = new T[capacity];
        }

        public void Push(T item)
        {
            if (Count >= buffer.Length)
            {
                throw new IndexOutOfRangeException();
            }

            buffer[current] = item;
            current = (current + 1) % buffer.Length;
            Count++;
        }

        public T Pop()
        {
            if (Count <= 0)
            {
                throw new IndexOutOfRangeException();
            }

            Count--;
            return buffer[(current + buffer.Length - Count - 1) % buffer.Length];
        }

        public bool PushAndPopWhenFull(T toPush, out T? popped)
        {
            if (Count >= buffer.Length)
            {
                popped = buffer[current];
                buffer[current] = toPush;
                current = (current + 1) % buffer.Length;
                return true;
            }

            buffer[current] = toPush;
            current = (current + 1) % buffer.Length;
            Count++;
            popped = default(T);
            return false;
        }
    }
}