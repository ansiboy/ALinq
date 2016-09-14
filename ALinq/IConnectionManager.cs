using System.Data.Common;
using ALinq.SqlClient;
using ALinq;

namespace ALinq
{
    internal interface IConnectionManager
    {
        // Methods
        void ReleaseConnection(IConnectionUser user);
        DbConnection UseConnection(IConnectionUser user);
        DbTransaction Transaction { get; }
    }
}