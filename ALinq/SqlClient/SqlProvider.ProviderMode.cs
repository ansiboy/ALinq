using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{


    public partial class SqlProvider
    {
        internal enum ProviderMode
        {
            NotYetDecided,
            Sql2000,
            Sql2005,
            SqlCE,
            Oracle,
            Access,
            MySql,
            Firebird,
            SQLite,
            OdpOracle,
            EffiProz,
            DB2,
            Pgsql
        }
    }
}
