using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ALinq.SqlClient
{
    internal abstract class SqlIdentifier
    {
        // Fields
        protected readonly DbCommandBuilder builder;
        //protected internal string ParameterPrefix = "@";
        //protected string QuotePrefix = "[";
        //protected string QuoteSuffix = "]";
        protected string SchemaSeparator = ".";
        protected char SchemaSeparatorChar = '.';

        internal abstract string ParameterPrefix { get; }
        internal abstract string QuotePrefix { get; }
        internal abstract string QuoteSuffix { get; }

        protected SqlIdentifier(DbCommandBuilder builder)
        {
            this.builder = builder;
            this.builder.QuotePrefix = QuotePrefix;
            this.builder.QuoteSuffix = QuoteSuffix;
        }

        // Methods
        internal virtual bool IsQuoted(string s)
        {
            if (s == null)
            {
                throw ALinq.Error.ArgumentNull("s");
            }
            if (s.Length < 2)
            {
                return false;
            }
            return (s.StartsWith(QuotePrefix, StringComparison.Ordinal) && s.EndsWith(QuoteSuffix, StringComparison.Ordinal));
        }

        internal virtual string QuoteCompoundIdentifier(string s)
        {
            if (s == null)
            {
                throw ALinq.Error.ArgumentNull("s");
            }
            if (s.StartsWith(ParameterPrefix, StringComparison.Ordinal))
            {
                return s;
            }
            if (IsQuoted(s))
            {
                return s;
            }
            if (!s.StartsWith(QuotePrefix, StringComparison.Ordinal) && s.EndsWith(QuoteSuffix, StringComparison.Ordinal))
            {
                int length = s.IndexOf('.');
                if (length < 0)
                {
                    return builder.QuoteIdentifier(s);
                }
                string str = s.Substring(0, length);
                string str2 = s.Substring(length + 1, (s.Length - length) - 1);
                if (!IsQuoted(str2))
                {
                    str2 = builder.QuoteIdentifier(str2);
                }
                return (QuoteCompoundIdentifier(str) + ('.' + str2));
            }
            if (s.StartsWith(QuotePrefix, StringComparison.Ordinal) && !s.EndsWith(QuoteSuffix, StringComparison.Ordinal))
            {
                int num2 = s.LastIndexOf('.');
                if (num2 < 0)
                {
                    return builder.QuoteIdentifier(s);
                }
                string str3 = s.Substring(0, num2);
                if (!IsQuoted(str3))
                {
                    str3 = builder.QuoteIdentifier(str3);
                }
                string str4 = s.Substring(num2 + 1, (s.Length - num2) - 1);
                return (str3 + ('.') + QuoteCompoundIdentifier(str4));
            }
            int index = s.IndexOf('.');
            if (index < 0)
            {
                return builder.QuoteIdentifier(s);
            }
            string str5 = s.Substring(0, index);
            string str6 = s.Substring(index + 1, (s.Length - index) - 1);
            return (QuoteCompoundIdentifier(str5) + ('.') + QuoteCompoundIdentifier(str6));
        }

        internal virtual string QuoteIdentifier(string s)
        {
            if (s == null)
            {
                throw ALinq.Error.ArgumentNull("s");
            }
            if (s.StartsWith(ParameterPrefix, StringComparison.Ordinal))
            {
                return s;
            }
            if (IsQuoted(s))
            {
                return s;
            }
            return builder.QuoteIdentifier(s);
        }

        internal virtual string UnquoteIdentifier(string quotedIdentifier)
        {
            if (string.IsNullOrEmpty(quotedIdentifier))
                Error.ArgumentNull(quotedIdentifier);

            Debug.Assert(quotedIdentifier != null);
            if ((quotedIdentifier.Length < 2) || (quotedIdentifier.StartsWith(QuotePrefix) == false) ||
                (quotedIdentifier.EndsWith(QuoteSuffix) == false))
            {
                return quotedIdentifier;
            }
            var len = quotedIdentifier.Length - QuotePrefix.Length - QuoteSuffix.Length;
            return quotedIdentifier.Substring(QuotePrefix.Length, len);//.Replace("\"\"", "\"");
        }

        internal IEnumerable<string> GetCompoundIdentifierParts(string s)
        {
            if (s.StartsWith(ParameterPrefix, StringComparison.Ordinal))
            {
                throw Error.ArgumentWrongValue("s");
            }

            var qutoe = QuoteCompoundIdentifier(s);
            string pattern = @"^(?<component>\[([^\]]|\]\])*\])(\.(?<component>\[([^\]]|\]\])*\]))*$"; ;
            if (this.QuotePrefix != "[" || this.QuoteSuffix != "]")
            {
                pattern = pattern.Replace(@"\[", this.QuotePrefix);
                pattern = pattern.Replace(@"\]", this.QuoteSuffix);
            }
            var match = Regex.Match(qutoe, pattern);
            if (match.Success)
            {
                var captures = match.Groups["component"].Captures;
                foreach (Capture capture in captures)
                {
                    yield return capture.Value;
                }
            }
        }

        internal virtual bool NeedToQuote(string name)
        {
            return true;
        }
    }

    abstract class SqlIdentifier<T1, T2> : SqlIdentifier
        where T1 : DbCommandBuilder, new()
        where T2 : SqlIdentifier, new()
    {
        private static SqlIdentifier instance;

        protected SqlIdentifier()
            : base(new T1())
        {
        }

        internal static SqlIdentifier Instance
        {
            get
            {
                if (instance == null)
                    instance = new T2();
                return instance;
            }
        }
    }
}