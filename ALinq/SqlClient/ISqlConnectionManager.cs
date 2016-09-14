using System.Data.Common;

namespace ALinq.SqlClient
{
    internal interface ISqlConnectionManager : IConnectionManager
    {
        void ClearConnection();
        void DisposeConnection();
        void ReleaseConnection(IConnectionUser user);
        DbConnection UseConnection(IConnectionUser user);
        bool AutoClose { get; set; }
        DbConnection Connection { get; }
        int MaxUsers { get; }
        DbTransaction Transaction { get; set; }
    }
}