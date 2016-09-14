using ALinq;

namespace ALinq.SqlClient
{
    internal interface ICompiledSubQuery
    {
        // Methods
        IExecuteResult Execute(IProvider provider, object[] parentArgs, object[] userArgs);
    }
}