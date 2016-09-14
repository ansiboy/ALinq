using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlValue : SqlSimpleTypeExpression
    {
        // Fields

        // Methods
        internal SqlValue(Type clrType, IProviderType sqlType, object value, bool isClientSpecified, Expression sourceExpression)
            : base(SqlNodeType.Value, clrType, sqlType, sourceExpression)
        {
#if DEBUG
            //if (sqlType.IsString && sqlType.GetType().Name != "AccessDataType")
            //    Debug.Assert(sqlType.Size > 0);
#endif
            this.Value = value;
            this.IsClientSpecified = isClientSpecified;
        }

        // Properties
        internal bool IsClientSpecified { get; set; }

        internal object Value { get; private set; }

        public override string Text
        {
            get
            {
                return Value.ToString();
            }
        }
    }
}