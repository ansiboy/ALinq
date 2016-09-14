using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlAssign : SqlStatement
    {
         // Fields
        private SqlExpression leftValue;
        private SqlExpression rightValue;

        // Methods
        internal SqlAssign(SqlExpression lValue, SqlExpression rValue, Expression sourceExpression)
            : base(SqlNodeType.Assign, sourceExpression)
        {
            this.LValue = lValue;
            this.RValue = rValue;
        }

        // Properties
        internal SqlExpression LValue
        {
            get
            {
                return leftValue;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((rightValue != null) && !value.ClrType.IsAssignableFrom(rightValue.ClrType))
                {
                    throw Error.ArgumentWrongType("value", rightValue.ClrType, value.ClrType);
                }
                leftValue = value;
            }
        }

        internal SqlExpression RValue
        {
            get
            {
                return rightValue;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((leftValue != null) && !leftValue.ClrType.IsAssignableFrom(value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", leftValue.ClrType, value.ClrType);
                }
                rightValue = value;
            }
        }
    }
}