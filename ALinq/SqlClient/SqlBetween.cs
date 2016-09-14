using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlBetween : SqlSimpleTypeExpression
    {
    
        // Fields
        private SqlExpression end;
        private SqlExpression expression;
        private SqlExpression start;

        // Methods
        internal SqlBetween(Type clrType, IProviderType sqlType, SqlExpression expr, SqlExpression start, SqlExpression end, Expression source)
            : base(SqlNodeType.Between, clrType, sqlType, source)
        {
            this.expression = expr;
            this.start = start;
            this.end = end;
        }

        // Properties
        internal SqlExpression End
        {
            get
            {
                return this.end;
            }
            set
            {
                this.end = value;
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
                this.expression = value;
            }
        }

        internal SqlExpression Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value;
            }
        }
    }
}