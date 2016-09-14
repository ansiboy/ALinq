using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlTypeCase : SqlExpression
    {
        // Fields
        private SqlExpression discriminator;
        private IProviderType sqlType;

        // Methods
        internal SqlTypeCase(Type clrType, IProviderType sqlType, MetaType rowType, 
                             SqlExpression discriminator, IEnumerable<SqlTypeCaseWhen> whens, Expression sourceExpression)
            : base(SqlNodeType.TypeCase, clrType, sourceExpression)
        {
            this.Whens = new List<SqlTypeCaseWhen>();
            this.Discriminator = discriminator;
            if (whens == null)
            {
                throw Error.ArgumentNull("whens");
            }
            this.Whens.AddRange(whens);
            if (this.Whens.Count == 0)
            {
                throw Error.ArgumentOutOfRange("whens");
            }
            this.sqlType = sqlType;
            this.RowType = rowType;
        }

        // Properties
        internal SqlExpression Discriminator
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.discriminator;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((this.discriminator != null) && (this.discriminator.ClrType != value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.discriminator.ClrType, value.ClrType);
                }
                this.discriminator = value;
            }
        }

        internal MetaType RowType { get; private set; }

        internal override IProviderType SqlType
        {
            get
            {
                return this.sqlType;
            }
        }

        internal List<SqlTypeCaseWhen> Whens { get; private set; }
    }
}