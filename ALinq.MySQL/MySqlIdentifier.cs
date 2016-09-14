using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ALinq.MySQL
{
    class MySqlIdentifier : ALinq.SqlClient.SqlIdentifier
    {
        private static MySqlIdentifier instance;

        protected MySqlIdentifier()
            : base(new MySql.Data.MySqlClient.MySqlCommandBuilder())
        {
            //this.QuotePrefix = "~";
            //this.QuoteSuffix = "~";
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
            get { return "`"; }
        }

        internal override string QuoteSuffix
        {
            get { return "`"; }
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
                    instance = new MySqlIdentifier();
                return instance;
            }
        }


    }
}
