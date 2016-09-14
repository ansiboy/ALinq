using System;
using System.Runtime.InteropServices;

namespace ALinq
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ItemList<T> where T : class
    {
        private T[] items;
        private int count;
        public int Count
        {
            get
            {
                return this.count;
            }
        }
        public T[] Items
        {
            get
            {
                return this.items;
            }
        }
        public T this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                this.items[index] = value;
            }
        }
        public void Add(T item)
        {
            if ((this.items == null) || (this.items.Length == this.count))
            {
                this.GrowItems();
            }
            this.items[this.count] = item;
            this.count++;
        }

        public bool Contains(T item)
        {
            return (this.IndexOf(item) >= 0);
        }

        public Enumerator GetEnumerator()
        {
            Enumerator enumerator;
            enumerator.items = this.items;
            enumerator.index = -1;
            enumerator.endIndex = this.count - 1;
            return enumerator;
        }

        public bool Include(T item)
        {
            if (this.LastIndexOf(item) >= 0)
            {
                return false;
            }
            this.Add(item);
            return true;
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.items[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            if ((this.items == null) || (this.items.Length == this.count))
            {
                this.GrowItems();
            }
            if (index < this.count)
            {
                Array.Copy(this.items, index, this.items, index + 1, this.count - index);
            }
            this.items[index] = item;
            this.count++;
        }

        public int LastIndexOf(T item)
        {
            int count = this.count;
            while (count > 0)
            {
                count--;
                if (this.items[count] == item)
                {
                    return count;
                }
            }
            return -1;
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index < 0)
            {
                return false;
            }
            this.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            this.count--;
            if (index < this.count)
            {
                Array.Copy(this.items, index + 1, this.items, index, this.count - index);
            }
            this.items[this.count] = default(T);
        }

        private void GrowItems()
        {
            Array.Resize<T>(ref this.items, (this.count == 0) ? 4 : (this.count * 2));
        }
        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            internal T[] items;
            internal int index;
            internal int endIndex;
            public bool MoveNext()
            {
                if (this.index == this.endIndex)
                {
                    return false;
                }
                this.index++;
                return true;
            }

            public T Current
            {
                get
                {
                    return this.items[this.index];
                }
            }
        }
    }
}