using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ALinq;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        internal class CompiledSubQuery : ICompiledSubQuery
        {
            // Fields
            private readonly IObjectReaderFactory factory;
            private readonly ReadOnlyCollection<SqlParameter> parameters;
            private QueryInfo queryInfo;
            public ICompiledSubQuery[] subQueries;

            // Methods
            public CompiledSubQuery(QueryInfo queryInfo, IObjectReaderFactory factory,
                                    ReadOnlyCollection<SqlParameter> parameters, ICompiledSubQuery[] subQueries)
            {
                this.queryInfo = queryInfo;
                this.factory = factory;
                this.parameters = parameters;
                this.subQueries = subQueries;
            }

            public IExecuteResult Execute(IProvider provider, object[] parentArgs, object[] userArgs)
            {
                if (((parentArgs == null) && (parameters != null)) && (parameters.Count != 0))
                {
                    throw Error.ArgumentNull("arguments");
                }
                var provider2 = provider as SqlProvider;
                if (provider2 == null)
                {
                    throw Error.ArgumentTypeMismatch("provider");
                }
                var list = new List<SqlParameterInfo>(this.queryInfo.Parameters);
                int index = 0;
                int count = parameters.Count;
                while (index < count)
                {
                    list.Add(new SqlParameterInfo(parameters[index], parentArgs[index]));
                    index++;
                }
                var info = new QueryInfo(queryInfo.Query, queryInfo.CommandText, list.AsReadOnly(),
                                         queryInfo.ResultShape, queryInfo.ResultType);
                return provider2.Execute(null, info, factory, parentArgs, userArgs, subQueries, null);
            }

            public QueryInfo QueryInfo
            {
                get { return queryInfo; }
                set { queryInfo = value; }
            }
        }
    }
}
