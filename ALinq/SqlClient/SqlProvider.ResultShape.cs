using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        internal enum ResultShape
        {
            Return,
            Singleton,
            Sequence,
            MultipleResults
        }
    }
}
