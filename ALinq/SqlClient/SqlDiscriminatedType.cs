using System;
using ALinq.Mapping;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlDiscriminatedType : SqlExpression
    {
        // Fields
        private SqlExpression discriminator;
        private IProviderType sqlType;
        private MetaType targetType;

        // Methods
        internal SqlDiscriminatedType(IProviderType sqlType, SqlExpression discriminator, MetaType targetType, Expression sourceExpression)
            : base(SqlNodeType.DiscriminatedType, typeof(Type), sourceExpression)
        {
            if (discriminator == null)
            {
                throw Error.ArgumentNull("discriminator");
            }
            this.discriminator = discriminator;
            this.targetType = targetType;
            this.sqlType = sqlType;
        }

        // Properties
        internal SqlExpression Discriminator
        {
            get
            {
                return this.discriminator;
            }
            set
            {
                this.discriminator = value;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.sqlType;
            }
        }

        internal MetaType TargetType
        {
            get
            {
                return this.targetType;
            }
        }

    }
}