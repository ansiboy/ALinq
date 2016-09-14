using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        internal class SingleResult<T> : ISingleResult<T>, IListSource
        {
            // Fields
            private IBindingList cachedList;
            private readonly DataContext context;
            private readonly IEnumerable<T> enumerable;
            private readonly ExecuteResult executeResult;

            // Methods
            internal SingleResult(IEnumerable<T> enumerable, ExecuteResult executeResult, DataContext context)
            {
                this.enumerable = enumerable;
                this.executeResult = executeResult;
                this.context = context;
            }

            public void Dispose()
            {
                executeResult.Dispose();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return enumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            IList IListSource.GetList()
            {
                if (cachedList == null)
                {
                    cachedList = BindingList.Create(context, this);
                }
                return cachedList;
            }

            // Properties
            public object ReturnValue
            {
                get { return executeResult.GetParameterValue("@RETURN_VALUE"); }
            }

            bool IListSource.ContainsListCollection
            {
                get { return false; }
            }
        }
    }
}
