using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlObjectType : SqlExpression
    {
         // Fields
        private SqlExpression obj;
        private IProviderType sqlType;

        // Methods
        internal SqlObjectType(SqlExpression obj, IProviderType sqlType, Expression sourceExpression)
            : base(SqlNodeType.ObjectType, typeof(Type), sourceExpression)
        {
            this.obj = obj;
            this.sqlType = sqlType;
        }

        internal void SetSqlType(IProviderType type)
        {
            this.sqlType = type;
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

        internal override IProviderType SqlType
        {
            get
            {
                return this.sqlType;
            }
        }

    }
}