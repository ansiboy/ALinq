using System;
using System.Collections.Generic;
using System.Linq;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        internal class MultipleResults : IMultipleResults
        {
            // Fields
            private readonly ExecuteResult executeResult;
            private readonly MetaFunction function;
            private bool isDisposed;
            private readonly SqlProvider provider;
            private readonly IObjectReaderSession session;

            // Methods
            public MultipleResults(SqlProvider provider, MetaFunction function, IObjectReaderSession session,
                                   ExecuteResult executeResult)
            {
                this.provider = provider;
                this.function = function;
                this.session = session;
                this.executeResult = executeResult;
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    if (executeResult != null)
                    {
                        executeResult.Dispose();
                    }
                    else
                    {
                        session.Dispose();
                    }
                }
            }

            IEnumerable<T> IMultipleResults.GetResult<T>()
            {
                MetaType rowType = null;
                Func<MetaType, bool> predicate = null;
                if (function != null)
                {
                    foreach (MetaType type2 in function.ResultRowTypes)
                    {
                        if (predicate == null)
                        {
                            predicate = (it => it.Type == typeof (T));
                        }
                        rowType = type2.InheritanceTypes.SingleOrDefault(predicate);
                        if (rowType != null)
                        {
                            break;
                        }
                    }
                }
                if (rowType == null)
                {
                    rowType = provider.services.Model.GetMetaType(typeof(T));
                }
                IObjectReader nextResult = provider.GetDefaultFactory(rowType).GetNextResult(session, false);
                if (nextResult == null)
                {
                    Dispose();
                    return null;
                }
                return new SingleResult<T>(new OneTimeEnumerable<T>((IEnumerator<T>)nextResult), executeResult,
                                           provider.services.Context);
            }

            // Properties
            object IFunctionResult.ReturnValue
            {
                get
                {
                    if (executeResult != null)
                    {
                        return executeResult.GetParameterValue("@RETURN_VALUE");
                    }
                    return null;
                }
            }
        }
    }
}
