using System.Data.Common;

namespace ALinq.SqlClient
{
    internal interface IObjectReaderFactory
    {
        // Methods
        IObjectReader Create(DbDataReader reader, bool disposeReader, IReaderProvider provider, object[] parentArgs,
                             object[] userArgs, ICompiledSubQuery[] subQueries);

        IObjectReader GetNextResult(IObjectReaderSession session, bool disposeReader);
    }
}