using System.Collections;
using System.Collections.Generic;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
	{
        internal class OneTimeEnumerable<T> : IEnumerable<T>
        {
            // Fields
            private IEnumerator<T> enumerator;
            private readonly IList<T> list;

            internal OneTimeEnumerable(T obj)
            {
                list = new List<T>(1) { obj };
                enumerator = list.GetEnumerator();
            }

            // Methods
            internal OneTimeEnumerable(IEnumerator<T> enumerator)
            {
                this.enumerator = enumerator;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (this.enumerator == null)
                {
                    throw Error.CannotEnumerateResultsMoreThanOnce();
                }
                IEnumerator<T> result = this.enumerator;
                this.enumerator = null;
                return result;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
	}
}
