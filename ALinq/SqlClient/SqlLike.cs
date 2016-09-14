using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlLike : SqlSimpleTypeExpression
    {
      
        // Fields
        private SqlExpression escape;
        private SqlExpression expression;
        private SqlExpression pattern;

        // Methods
        internal SqlLike(Type clrType, IProviderType sqlType, SqlExpression expr, SqlExpression pattern, SqlExpression escape, Expression source)
            : base(SqlNodeType.Like, clrType, sqlType, source)
        {
            if (expr == null)
            {
                throw Error.ArgumentNull("expr");
            }
            if (pattern == null)
            {
                throw Error.ArgumentNull("pattern");
            }
            this.Expression = expr;
            this.Pattern = pattern;
            this.Escape = escape;
        }

        // Properties
        internal SqlExpression Escape
        {
            get
            {
                return this.escape;
            }
            set
            {
                if ((value != null) && (value.ClrType != typeof(string)))
                {
                    throw Error.ArgumentWrongType("value", "string", value.ClrType);
                }
                this.escape = value;
            }
        }

        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if (value.ClrType != typeof(string))
                {
                    throw Error.ArgumentWrongType("value", "string", value.ClrType);
                }
                this.expression = value;
            }
        }

        internal SqlExpression Pattern
        {
            get
            {
                return this.pattern;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if (value.ClrType != typeof(string))
                {
                    throw Error.ArgumentWrongType("value", "string", value.ClrType);
                }
                this.pattern = value;
            }
        }

    }
}