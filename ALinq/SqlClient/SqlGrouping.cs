using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlGrouping : SqlSimpleTypeExpression
    {
        // Fields
        private SqlExpression group;
        private SqlExpression key;

        // Methods
        internal SqlGrouping(Type clrType, IProviderType sqlType, SqlExpression key, SqlExpression group, Expression sourceExpression)
            : base(SqlNodeType.Grouping, clrType, sqlType, sourceExpression)
        {
            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }
            if (group == null)
            {
                throw Error.ArgumentNull("group");
            }
            this.key = key;
            this.group = group;
        }

        // Properties
        internal SqlExpression Group
        {
            get
            {
                return this.group;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if (value.ClrType != this.group.ClrType)
                {
                    throw Error.ArgumentWrongType("value", this.group.ClrType, value.ClrType);
                }
                this.group = value;
            }
        }

        internal SqlExpression Key
        {
            get
            {
                return this.key;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if (!this.key.ClrType.IsAssignableFrom(value.ClrType) && !value.ClrType.IsAssignableFrom(this.key.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.key.ClrType, value.ClrType);
                }
                this.key = value;
            }
        }

    }
}