using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ALinq.SqlClient
{
    internal class SqlMember : SqlSimpleTypeExpression
    {
       
        // Fields
        private SqlExpression expression;
        private readonly MemberInfo member;

        // Methods
        internal SqlMember(Type clrType, IProviderType sqlType, SqlExpression expr, MemberInfo member)
            : base(SqlNodeType.Member, clrType, sqlType, expr.SourceExpression)
        {
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
                if (!this.member.ReflectedType.IsAssignableFrom(value.ClrType) && !value.ClrType.IsAssignableFrom(this.member.ReflectedType))
                {
                    throw Error.MemberAccessIllegal(this.member, this.member.ReflectedType, value.ClrType);
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

        public override string Text
        {
            get
            {
                return this.Member.Name;
            }
        }

    }
}