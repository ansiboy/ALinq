using System;

namespace ALinq.SqlClient
{
    internal class SqlTypeCaseWhen
    {
        // Fields
        private SqlExpression match;
        private SqlExpression @new;

        // Methods
        internal SqlTypeCaseWhen(SqlExpression match, SqlExpression typeBinding)
        {
            this.Match = match;
            this.TypeBinding = typeBinding;
        }

        // Properties
        internal SqlExpression Match
        {
            get
            {
                return this.match;
            }
            set
            {
                if (((this.match != null) && (value != null)) && (this.match.ClrType != value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.match.ClrType, value.ClrType);
                }
                this.match = value;
            }
        }

        internal SqlExpression TypeBinding
        {
            get
            {
                return this.@new;
            }
            set
            {
                this.@new = value;
            }
        }

    }
}