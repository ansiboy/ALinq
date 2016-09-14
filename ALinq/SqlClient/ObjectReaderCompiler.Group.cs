using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IEnumerator=System.Collections.IEnumerator;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler
    {
        internal class Group<K, T> : IGrouping<K, T>
        {
            // Fields
            private readonly IEnumerable<T> items;
            private readonly K key;

            // Methods
            internal Group(K key, IEnumerable<T> items)
            {
                this.key = key;
                this.items = items;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            // Properties
            K IGrouping<K, T>.Key
            {
                get { return key; }
            }
        }
    }
}
