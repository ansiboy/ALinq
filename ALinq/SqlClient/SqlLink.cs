using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlLink : SqlSimpleTypeExpression
    {
         // Fields
        private SqlExpression expansion;
        private SqlExpression expression;
        private object id;
        private List<SqlExpression> keyExpressions;
        private MetaDataMember member;
        private MetaType rowType;

        // Methods
        internal SqlLink(object id, MetaType rowType, Type clrType, IProviderType sqlType, SqlExpression expression, MetaDataMember member, IEnumerable<SqlExpression> keyExpressions, SqlExpression expansion, Expression sourceExpression)
            : base(SqlNodeType.Link, clrType, sqlType, sourceExpression)
        {
            this.id = id;
            this.rowType = rowType;
            this.expansion = expansion;
            this.expression = expression;
            this.member = member;
            this.keyExpressions = new List<SqlExpression>();
            if (keyExpressions != null)
            {
                this.keyExpressions.AddRange(keyExpressions);
            }
        }

        // Properties
        internal SqlExpression Expansion
        {
            get
            {
                return this.expansion;
            }
            set
            {
                this.expansion = value;
            }
        }

        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
            }
        }

        internal object Id
        {
            get
            {
                return this.id;
            }
        }

        internal List<SqlExpression> KeyExpressions
        {
            get
            {
                return this.keyExpressions;
            }
        }

        internal MetaDataMember Member
        {
            get
            {
                return this.member;
            }
        }

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

    }
}