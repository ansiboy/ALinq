using System;
using System.Globalization;
using System.Linq;
using ALinq.Oracle;
using ALinq.SqlClient;

namespace ALinq.DB2
{
    class DB2Identifier : SqlIdentifier
    {
        private static DB2Identifier instance;

        private DB2Identifier()
            : base(new IBM.Data.DB2.DB2CommandBuilder())
        {

        }

        internal override string ParameterPrefix
        {
            get { return "@"; }
        }

        internal override string QuotePrefix
        {
            get
            {
                //return "\"";
                return builder.QuotePrefix;
            }
        }

        internal override string QuoteSuffix
        {
            get
            {
                //return "\"";
                return builder.QuoteSuffix;
            }
        }

        internal static SqlIdentifier Instance
        {
            get
            {

                if (instance == null)
                    instance = new DB2Identifier();
                return instance;
            }
        }

        //internal override bool IsQuoted(string s)
        //{
        //    return true;
        //}

        //internal override string QuoteIdentifier(string s)
        //{
        //    //if (keyWords.Contains(s, StringComparer.Create(CultureInfo.CurrentCulture, true)))
        //    //    return base.QuoteIdentifier(s);
        //    return s;
        //}

        //internal override string QuoteCompoundIdentifier(string s)
        //{
        //    return s;
        //}

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