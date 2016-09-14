using System;
using System.Diagnostics;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlBinary : SqlSimpleTypeExpression
    {

        // Fields
        private SqlExpression left;
        private readonly MethodInfo method;
        private SqlExpression right;

        // Methods
        internal SqlBinary(SqlNodeType nt, Type clrType, IProviderType sqlType, SqlExpression left, SqlExpression right)
            : this(nt, clrType, sqlType, left, right, null)
        {
        }

        internal SqlBinary(SqlNodeType nt, Type clrType, IProviderType sqlType, SqlExpression left, SqlExpression right, MethodInfo method)
            : base(nt, clrType, sqlType, right.SourceExpression)
        {
            switch (nt)
            {
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.And:
                case SqlNodeType.Add:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.Or:
                case SqlNodeType.Sub:
                    this.Left = left;
                    this.Right = right;
                    this.method = method;
                    return;
            }
            throw Error.UnexpectedNode(nt);
        }

        // Properties
        internal SqlExpression Left
        {
            get
            {
                return this.left;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.left = value;
            }
        }

        internal MethodInfo Method
        {
            get
            {
                return this.method;
            }
        }

        internal SqlExpression Right
        {
            get
            {
                return this.right;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.right = value;
            }
        }

        public override string Text
        {
            get
            {
                return string.Format("{0} {1} {2}", Left.Text, NodeType, Right.Text);
            }
        }
    }
}