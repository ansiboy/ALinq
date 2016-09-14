using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlMemberAssign : SqlNode
    {
        // Fields
        private SqlExpression expression;
        private readonly MemberInfo member;

        // Methods
        internal SqlMemberAssign(MemberInfo member, SqlExpression expr)
            : base(SqlNodeType.MemberAssign, expr.SourceExpression)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }
            this.member = member;
            this.Expression = expr;
        }

        // Properties
        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.expression = value;
            }
        }

        internal MemberInfo Member
        {
            get
            {
                return this.member;
            }
        }

    }
}