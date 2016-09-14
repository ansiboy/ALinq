using ALinq.SqlClient;
using FirebirdSql.Data.FirebirdClient;

namespace ALinq.Firebird
{
    class FirebirdIdentifier : SqlClient.SqlIdentifier
    {
        private static FirebirdIdentifier instance;

        private FirebirdIdentifier()
            : base(new FbCommandBuilder())
        {
        }

        internal override string ParameterPrefix
        {
            get { return "@"; }
        }

        internal override string QuotePrefix
        {
            get { return "\""; }
        }

        internal override string QuoteSuffix
        {
            get { return "\""; }
        }

        //internal override string QuoteIdentifier(string s)
        //{
        //    if (s == "Key")
        //        return base.QuoteIdentifier(s);
        //    return s;
        //}

        //internal override string QuoteCompoundIdentifier(string s)
        //{
        //    if (s == "EMPTY")
        //        return base.QuoteCompoundIdentifier(s);
        //    return s;
        //}

        [System.Diagnostics.DebuggerStepThrough]
        internal override bool NeedToQuote(string name)
        {
            return FirebirdKeywords.Contains(name);
        }

        internal static SqlClient.SqlIdentifier Instance
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                if (instance == null)
                    instance = new FirebirdIdentifier();
                return instance;
            }
        }


    }
}