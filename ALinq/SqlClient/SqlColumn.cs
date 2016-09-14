using System;
using System.Diagnostics;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlColumn : SqlExpression
    {
        // Fields
        private SqlExpression expression;
        private readonly IProviderType sqlType;
        private MetaDataMember metaMember;
        private SqlAlias alias;

        // Methods
        internal SqlColumn(string name, SqlExpression expr)
            : this(expr.ClrType, expr.SqlType, name, null, expr, expr.SourceExpression)
        {
            Debug.Assert(expr.SqlType != null);

        }

        //internal SqlColumn(string name, Expression expr)
        //    : this(expr.ClrType, expr.SqlType, name, null, expr, expr.SourceExpression)
        //{
        //}

        internal SqlColumn(Type clrType, IProviderType sqlType, string name, MetaDataMember member, SqlExpression expr, Expression sourceExpression)
            : base(SqlNodeType.Column, clrType, sourceExpression)
        {
            if (typeof(Type).IsAssignableFrom(clrType))
            {
                throw Error.ArgumentWrongValue("clrType");
            }
            this.name = name;
            MetaMember = member;
            Expression = expr;
            Ordinal = -1;
            if (sqlType == null)
            {
                throw Error.ArgumentNull("sqlType");
            }
            this.sqlType = sqlType;


        }

        // Properties
        internal virtual SqlAlias Alias
        {
            get { return alias; }
            set { alias = value; }
        }

        internal SqlExpression Expression
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return expression;
            }
            set
            {
                if (value != null)
                {
                    Debug.Assert(value.SqlType != null);

                    if (!ClrType.IsAssignableFrom(value.ClrType))
                    {
                        throw Error.ArgumentWrongType("value", ClrType, value.ClrType);
                    }
                    var ref2 = value as SqlColumnRef;
                    if ((ref2 != null) && (ref2.Column == this))
                    {
                        throw Error.ColumnCannotReferToItself();
                    }
                }
                expression = value;
            }
        }


        internal MetaDataMember MetaMember
        {
            get { return metaMember; }
            private set { metaMember = value; }
        }

        private string name;

        internal string Name
        {
            get { return name; }
            set { name = value; }
        }

        internal int Ordinal { get; set; }

        internal override IProviderType SqlType
        {
            get
            {
                return expression != null ? expression.SqlType : sqlType;
            }
        }

        public override string Text
        {
            get
            {
                return this.Name;
            }
        }
    }

    //internal class SqlDynamicColumn : SqlColumn
    //{
    //    private IProviderType sqlType;
    //    internal SqlDynamicColumn(Type clrType, IProviderType sqlType, string name, MetaDataMember member, SqlExpression expr, Expression sourceExpression)
    //        : base(clrType, sqlType, name, member, expr, sourceExpression)
    //    {
    //        this.sqlType = base.SqlType;

    //        //var col = new SqlColumn(clrType, sqlType, name, member, expr, sourceExpression);
    //        //Expression = new SqlColumnRef(col);
    //    }

    //    internal SqlDynamicColumn(string name, SqlExpression expr)
    //        : base(name, expr)
    //    {
    //        this.sqlType = base.SqlType;
    //    }

    //    internal override IProviderType SqlType
    //    {
    //        get
    //        {
    //            return this.sqlType;
    //        }
    //    }

    //    internal void SetSqlType(IProviderType value)
    //    {
    //        this.sqlType = value;
    //    }



    //}
}