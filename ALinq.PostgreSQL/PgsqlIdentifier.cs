using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Npgsql;

namespace ALinq.PostgreSQL
{
    class PgsqlIdentifier : SqlClient.SqlIdentifier
    {
        private static PgsqlIdentifier instance;

        public PgsqlIdentifier()
            : base(new NpgsqlCommandBuilder())
        {
        }

        public new static string QuoteIdentifier(string s)
        {
            return Instance.QuoteIdentifier(s);
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

        public new static string QuoteCompoundIdentifier(string s)
        {
            return Instance.QuoteCompoundIdentifier(s);
        }

        internal static ALinq.SqlClient.SqlIdentifier Instance
        {
            get
            {
                if (instance == null)
                    instance = new PgsqlIdentifier();
                return instance;
            }
        }

        internal override bool NeedToQuote(string name)
        {
            var index = name.LastIndexOf('.');
            string columnName = index > 0 ? name.Substring(index + 1) : name;

            if (IsQuoted(columnName))
                return false;

            return PgsqlKeywords.Contains(columnName);
        }
    }
}
