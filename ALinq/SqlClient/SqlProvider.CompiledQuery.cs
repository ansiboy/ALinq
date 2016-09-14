using ALinq;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        private class CompiledQuery : ICompiledQuery
        {
            // Fields
            private readonly IObjectReaderFactory factory;
            private readonly DataLoadOptions originalShape;
            private readonly Expression query;
            private readonly QueryInfo[] queryInfos;
            private readonly ICompiledSubQuery[] subQueries;

            // Methods
            internal CompiledQuery(SqlProvider provider, Expression query, QueryInfo[] queryInfos, IObjectReaderFactory factory, ICompiledSubQuery[] subQueries)
            {
                originalShape = provider.services.Context.LoadOptions;
                this.query = query;
                this.queryInfos = queryInfos;
                this.factory = factory;
                this.subQueries = subQueries;
            }

            private static bool AreEquivalentShapes(DataLoadOptions shape1, DataLoadOptions shape2)
            {
                if (shape1 == shape2)
                {
                    return true;
                }
                if (shape1 == null)
                {
                    return shape2.IsEmpty;
                }
                if (shape2 == null)
                {
                    return shape1.IsEmpty;
                }
                return (shape1.IsEmpty && shape2.IsEmpty);
            }

            public IExecuteResult Execute(IProvider provider, object[] arguments)
            {
                if (provider == null)
                {
                    throw Error.ArgumentNull("provider");
                }
                var provider2 = provider as SqlProvider;
                if (provider2 == null)
                {
                    throw Error.ArgumentTypeMismatch("provider");
                }
                if (!AreEquivalentShapes(originalShape, provider2.services.Context.LoadOptions))
                {
                    throw Error.CompiledQueryAgainstMultipleShapesNotSupported();
                }
                return provider2.ExecuteAll(query, this.queryInfos, this.factory, arguments, this.subQueries);
            }
        }
 

    }
}
