using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal abstract class SqlExpression : SqlNode
    {
        // Fields

        // Methods
        internal SqlExpression(SqlNodeType nodeType, Type clrType, Expression sourceExpression)
            : base(nodeType, sourceExpression)
        {
            this.ClrType = clrType;
        }

        internal void SetClrType(Type type)
        {
            this.ClrType = type;
        }

        // Properties
        internal Type ClrType { get; private set; }

        internal abstract IProviderType SqlType { get; }

        internal bool IsConstantColumn
        {
            get
            {
                if (base.NodeType == SqlNodeType.Column)
                {
                    var column = (SqlColumn)this;
                    if (column.Expression != null)
                    {
                        return column.Expression.IsConstantColumn;
                    }
                }
                else
                {
                    if (base.NodeType == SqlNodeType.ColumnRef)
                    {
                        return ((SqlColumnRef)this).Column.IsConstantColumn;
                    }
                    if (base.NodeType == SqlNodeType.OptionalValue)
                    {
                        return ((SqlOptionalValue)this).Value.IsConstantColumn;
                    }
                    if ((base.NodeType == SqlNodeType.Value) || (base.NodeType == SqlNodeType.Parameter))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


    }


}