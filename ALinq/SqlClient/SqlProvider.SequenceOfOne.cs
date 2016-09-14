using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IEnumerable=System.Collections.IEnumerable;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
	{
        private class SequenceOfOne<T> : IEnumerable<T>
        {
            // Fields
            private readonly T[] sequence;

            // Methods
            internal SequenceOfOne(T value)
            {
                sequence = new[] { value };
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>)sequence).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
	}
}
