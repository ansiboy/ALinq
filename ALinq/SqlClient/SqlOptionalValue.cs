using System;

namespace ALinq.SqlClient
{
    internal class SqlOptionalValue : SqlSimpleTypeExpression
    {
       
        // Fields
        private SqlExpression expressionValue;
        private SqlExpression hasValue;

        // Methods
        internal SqlOptionalValue(SqlExpression hasValue, SqlExpression value)
            : base(SqlNodeType.OptionalValue, value.ClrType, value.SqlType, value.SourceExpression)
        {
            this.HasValue = hasValue;
            this.Value = value;
        }

        // Properties
        internal SqlExpression HasValue
        {
            get
            {
                return this.hasValue;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.hasValue = value;
            }
        }

        internal SqlExpression Value
        {
            get
            {
                return this.expressionValue;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if (value.ClrType != base.ClrType)
                {
                    throw Error.ArgumentWrongType("value", base.ClrType, value.ClrType);
                }
                this.expressionValue = value;
            }
        }

    }
}