using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlNew : SqlSimpleTypeExpression
    {
        // Fields
        private readonly List<MemberInfo> argMembers;
        private readonly List<SqlExpression> args;
        private readonly ConstructorInfo constructor;
        private readonly List<SqlMemberAssign> members;
        private readonly MetaType metaType;

        // Methods
        internal SqlNew(MetaType metaType, IProviderType sqlType, ConstructorInfo cons, IEnumerable<SqlExpression> args, IEnumerable<MemberInfo> argMembers, IEnumerable<SqlMemberAssign> members, Expression sourceExpression)
            : base(SqlNodeType.New, metaType.Type, sqlType, sourceExpression)
        {
            this.metaType = metaType;
            if ((cons == null) && metaType.Type.IsClass)
            {
                throw Error.ArgumentNull("cons");
            }
            this.constructor = cons;
            this.args = new List<SqlExpression>();
            this.argMembers = new List<MemberInfo>();
            this.members = new List<SqlMemberAssign>();
            if (args != null)
            {
                this.args.AddRange(args);
            }
            if (argMembers != null)
            {
                this.argMembers.AddRange(argMembers);
            }
            if (members != null)
            {
                this.members.AddRange(members);
            }
        }


        internal SqlExpression Find(MemberInfo mi)
        {
            int num = 0;
            int count = this.argMembers.Count;
            while (num < count)
            {
                MemberInfo info = this.argMembers[num];
                if (info.Name == mi.Name)
                {
                    return this.args[num];
                }
                num++;
            }
            foreach (SqlMemberAssign assign in this.Members)
            {
                if (assign.Member.Name == mi.Name)
                {
                    return assign.Expression;
                }
            }
            return null;
        }
        // Properties
        internal List<MemberInfo> ArgMembers
        {
            get
            {
                return this.argMembers;
            }
        }

        internal List<SqlExpression> Args
        {
            get
            {
                return this.args;
            }
        }

        internal ConstructorInfo Constructor
        {
            get
            {
                return this.constructor;
            }
        }

        internal List<SqlMemberAssign> Members
        {
            get
            {
                return this.members;
            }
        }

        internal MetaType MetaType
        {
            get
            {
                return this.metaType;
            }
        }

        public override string Text
        {
            get
            {
                var txt = string.Join(",", ArgMembers.Select(o => o.Name).ToArray());
                return string.Format("({0})", txt);
            }
        }
    }
}