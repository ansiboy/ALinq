using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlClientParameter : SqlSimpleTypeExpression
    {
         // Fields
        private LambdaExpression accessor;

        // Methods
        internal SqlClientParameter(Type clrType, IProviderType sqlType, LambdaExpression accessor, Expression sourceExpression)
            : base(SqlNodeType.ClientParameter, clrType, sqlType, sourceExpression)
        {
            this.accessor = accessor;
        }

        // Properties
        internal LambdaExpression Accessor
        {
            get
            {
                return this.accessor;
            }
        }

    }
}