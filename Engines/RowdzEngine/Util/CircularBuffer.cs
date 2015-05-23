using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace HexLight.Engine.Util
{

    class CircularBuffer<T>
    {
        private T[] buffer;
        private int bufferIndex;
        private int bufferSize;
        private int size;

        public int Count { get { return size; } }

        private int calculateIndex(int index)
        {
            int i = index + bufferIndex;
            if (i >= bufferIndex) i -= bufferIndex;
            return i;
        }

        public T this[int index]
        {
            get { return buffer[calculateIndex(index)]; }
            set { buffer[calculateIndex(index)] = value; }
        }

        public CircularBuffer(int bufferSize)
        {
            this.bufferSize = bufferSize;

            buffer = new T[bufferSize];
            bufferIndex = 0;
            size = 0;
        }

        public void Add(T item)
        {
            this.buffer[bufferIndex++] = item;

            if (size < bufferSize)
                size++;

            if (bufferIndex == bufferSize)
                bufferIndex = 0;
        }

        public static float Average(CircularBuffer<float> buffer)
        {
            float sum = 0.0f;
            for (int i = 0; i < buffer.Count; i++)
                sum += buffer[i];
            return sum / (float)buffer.Count;
        }
    }

    /*class CircularBuffer<T>
    {
        private T[] buffer;
        private int bufferIndex;
        private int size;

        public bool IsReadOnly { get { return false; } }

        private int calculateIndex(int index)
        {
            int i = index + bufferIndex;
            if (i >= bufferIndex) i -= bufferIndex;
            return i;
        }

        public T this[int index] {
            get { return buffer[calculateIndex(index)]; }
            set { buffer[calculateIndex(index)] = value; }
        }
        
        public CircularBuffer(int bufferSize)
        {
            //this.Count = bufferSize;

            buffer = new T[bufferSize];
            Clear();

            
        }

        [System.ComponentModel.EditorBrowsable(EditorBrowsableState.Never)]
        new public bool Remove(T item) { return false; }

        



    }*/


}
