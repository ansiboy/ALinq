using System.Data.SQLite;

namespace ALinq.SQLite
{
    class SQLiteIdentifier : SqlClient.SqlIdentifier
    {
        public SQLiteIdentifier()
            : base(new SQLiteCommandBuilder())
        {
        }

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
