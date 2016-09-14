using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlDiscriminatorOf : SqlSimpleTypeExpression
    {
        // Fields
        private SqlExpression obj;

        // Methods
        internal SqlDiscriminatorOf(SqlExpression obj, Type clrType, IProviderType sqlType, Expression sourceExpression)
            : base(SqlNodeType.DiscriminatorOf, clrType, sqlType, sourceExpression)
        {
            this.obj = obj;
        }

        // Properties
        internal SqlExpression Object
        {
            get
            {
                return this.obj;
            }
            set
            {
                this.obj = value;
            }
        }

    }
}