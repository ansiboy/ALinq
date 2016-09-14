using System;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlUnion : SqlNode
    {
        // Fields
        private bool all;
        private SqlNode left;
        private SqlNode right;

        // Methods
        internal SqlUnion(SqlNode left, SqlNode right, bool all)
            : base(SqlNodeType.Union, right.SourceExpression)
        {
            this.Left = left;
            this.Right = right;
            this.All = all;
        }

        internal Type GetClrType()
        {
            SqlExpression left = this.Left as SqlExpression;
            if (left != null)
            {
                return left.ClrType;
            }
            SqlSelect select = this.Left as SqlSelect;
            if (select == null)
            {
                throw Error.CouldNotGetClrType();
            }
            return select.Selection.ClrType;
        }

        internal IProviderType GetSqlType()
        {
            SqlExpression left = this.Left as SqlExpression;
            if (left != null)
            {
                return left.SqlType;
            }
            SqlSelect select = this.Left as SqlSelect;
            if (select == null)
            {
                throw Error.CouldNotGetSqlType();
            }
            return select.Selection.SqlType;
        }

        private void Validate(SqlNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }
            if ((!(node is SqlExpression) && !(node is SqlSelect)) && !(node is SqlUnion))
            {
                throw Error.UnexpectedNode(node.NodeType);
            }
        }

        // Properties
        internal bool All
        {
            get
            {
                return this.all;
            }
            set
            {
                this.all = value;
            }
        }

        internal SqlNode Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.Validate(value);
                this.left = value;
            }
        }

        internal SqlNode Right
        {
            get
            {
                return this.right;
            }
            set
            {
                this.Validate(value);
                this.right = value;
            }
        }

    }
}