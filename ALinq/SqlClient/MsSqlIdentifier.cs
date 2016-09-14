using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace ALinq.SqlClient
{
    class MsSqlIdentifier : SqlIdentifier
    {
        // Fields
        //private static SqlIdentifier instance;

        private MsSqlIdentifier(DbCommandBuilder builder)
            : base(builder)
        {

        }

        public MsSqlIdentifier()
            : this(new SqlCommandBuilder())
        {

        }

        //internal static SqlIdentifier Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = new MsSqlIdentifier(new SqlCommandBuilder());
        //        return instance;
        //    }
        //}
        // Methods
        internal override string ParameterPrefix
        {
            get { return "@"; }
        }

        internal override string QuotePrefix
        {
            get { return "["; }
        }

        internal override string QuoteSuffix
        {
            get { return "]"; }
        }
    }
}