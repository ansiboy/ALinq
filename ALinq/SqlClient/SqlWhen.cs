using System;

namespace ALinq.SqlClient
{
    internal class SqlWhen
    {
        // Fields
        private SqlExpression matchExpression;
        private SqlExpression valueExpression;

        // Methods
        internal SqlWhen(SqlExpression match, SqlExpression value)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }
            this.Match = match;
            this.Value = value;
        }

        // Properties
        internal SqlExpression Match
        {
            get
            {
                return this.matchExpression;
            }
            set
            {
                if (((this.matchExpression != null) && (value != null)) && (this.matchExpression.ClrType != value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.matchExpression.ClrType, value.ClrType);
                }
                this.matchExpression = value;
            }
        }

        internal SqlExpression Value
        {
            get
            {
                return this.valueExpression;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((this.valueExpression != null) && !this.valueExpression.ClrType.IsAssignableFrom(value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.valueExpression.ClrType, value.ClrType);
                }
                this.valueExpression = value;
            }
        }
    }

    
}