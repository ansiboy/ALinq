using System;
using System.Data.Common;

namespace ALinq.SqlClient
{
    internal interface IObjectReaderCompiler
    {
        // Methods
        IObjectReaderFactory Compile(SqlExpression expression, Type elementType);

        IObjectReaderSession CreateSession(DbDataReader reader, IReaderProvider provider, object[] parentArgs,
                                           object[] userArgs, ICompiledSubQuery[] subQueries);
    }
}