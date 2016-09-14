using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlJoinedCollection : SqlSimpleTypeExpression
    {
           // Fields
        private SqlExpression count;
        private SqlExpression expression;

        // Methods
        internal SqlJoinedCollection(Type clrType, IProviderType sqlType, SqlExpression expression, SqlExpression count, Expression sourceExpression)
            : base(SqlNodeType.JoinedCollection, clrType, sqlType, sourceExpression)
        {
            this.expression = expression;
            this.count = count;
        }

        // Properties
        internal SqlExpression Count
        {
            get
            {
                return this.count;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if (value.ClrType != typeof(int))
                {
                    throw Error.ArgumentWrongType(value, typeof(int), value.ClrType);
                }
                this.count = value;
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
                if ((value == null) || ((this.expression != null) && (this.expression.ClrType != value.ClrType)))
                {
                    throw Error.ArgumentWrongType(value, this.expression.ClrType, value.ClrType);
                }
                this.expression = value;
            }
        }

    }
}