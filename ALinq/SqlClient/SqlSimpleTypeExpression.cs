using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal abstract class SqlSimpleTypeExpression : SqlExpression
    {

        // Fields
        private IProviderType sqlType;

        // Methods
        internal SqlSimpleTypeExpression(SqlNodeType nodeType, Type clrType, IProviderType sqlType, Expression sourceExpression)
            : base(nodeType, clrType, sourceExpression)
        {
            this.sqlType = sqlType;
        }

        internal void SetSqlType(IProviderType type)
        {
            this.sqlType = type;
        }

        // Properties
        internal override IProviderType SqlType
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.sqlType;
            }
        }

    }
}