using System;
using System.Diagnostics;
using System.Reflection;

namespace ALinq.SqlClient
{
    [DebuggerDisplay("{column.Name}")]
    internal class SqlColumnRef : SqlExpression
    {     // Fields
        private SqlColumn column;

        // Methods
        internal SqlColumnRef(SqlColumn col)
            : base(SqlNodeType.ColumnRef, col.ClrType, col.SourceExpression)
        {
            this.column = col;
        }

        public override bool Equals(object obj)
        {
            SqlColumnRef ref2 = obj as SqlColumnRef;
            return ((ref2 != null) && (ref2.Column == this.column));
        }

        public override int GetHashCode()
        {
            return this.column.GetHashCode();
        }

        internal SqlColumn GetRootColumn()
        {
            SqlColumn column = this.column;
            while ((column.Expression != null) && (column.Expression.NodeType == SqlNodeType.ColumnRef))
            {
                column = ((SqlColumnRef)column.Expression).Column;
            }
            return column;
        }

        // Properties
        internal SqlColumn Column
        {
            get
            {
                return this.column;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.column.SqlType;
            }
        }

        public override string Text
        {
            get
            {
                if (column != null)
                    return column.Name;
                return base.Text;
            }
        }
    }
}