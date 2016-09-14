using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ALinq.EffiProz
{
    class EfzIdentifier : ALinq.SqlClient.SqlIdentifier
    {
        public EfzIdentifier()
            : base(new System.Data.EffiProz.EfzCommandBuilder())
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

        internal override bool NeedToQuote(string name)
        {
            //return false;
            return base.NeedToQuote(name);
        }

        internal override string QuoteIdentifier(string s)
        {
            //return s;
            return base.QuoteIdentifier(s);
        }

        internal override string QuoteCompoundIdentifier(string s)
        {
            //return s;
            return base.QuoteCompoundIdentifier(s);
        }
    }
}
