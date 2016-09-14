using ALinq.SqlClient;

namespace ALinq.Oracle
{
    class OracleIdentifier : SqlIdentifier
    {
        private static OracleIdentifier instance;

        private OracleIdentifier()
            : base(new System.Data.OracleClient.OracleCommandBuilder())
        {

        }

        internal override string ParameterPrefix
        {
            get { return ":"; }
        }

        internal override string QuotePrefix
        {
            get
            {
                return "\"";
                //return builder.QuotePrefix;
            }
        }

        internal override string QuoteSuffix
        {
            get
            {
                return "\"";
                //return builder.QuoteSuffix;
            }
        }

        internal static SqlIdentifier Instance
        {
            get
            {

                if (instance == null)
                    instance = new OracleIdentifier();
                return instance;
            }
        }

        internal override bool NeedToQuote(string name)
        {
            var index = name.LastIndexOf('.');
            string columnName = index > 0 ? name.Substring(index + 1) : name;

            if (IsQuoted(columnName) == false && OracleKeywords.Contains(columnName))
                return true;

            return false;
        }

    }
}
