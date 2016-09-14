using System;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlSubSelect : SqlSimpleTypeExpression
    {  // Fields
        private SqlSelect select;

        // Methods
        internal SqlSubSelect(SqlNodeType nt, Type clrType, IProviderType sqlType, SqlSelect select)
            : base(nt, clrType, sqlType, select.SourceExpression)
        {
            switch (nt)
            {
                case SqlNodeType.Element:
                case SqlNodeType.Exists:
                case SqlNodeType.Multiset:
                case SqlNodeType.ScalarSubSelect:
                    this.Select = select;
                    return;
            }
            throw Error.UnexpectedNode(nt);
        }

        // Properties
        internal SqlSelect Select
        {
            get
            {
                return this.select;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.select = value;
            }
        }

    }
}