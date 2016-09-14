using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlSearchedCase : SqlExpression
    {
         // Fields
        private SqlExpression @else;
        private readonly List<SqlWhen> whens;

        // Methods
        internal SqlSearchedCase(Type clrType, IEnumerable<SqlWhen> whens, SqlExpression @else, Expression sourceExpression)
            : base(SqlNodeType.SearchedCase, clrType, sourceExpression)
        {
            if (whens == null)
            {
                throw Error.ArgumentNull("whens");
            }
            this.whens = new List<SqlWhen>(whens);
            if (this.whens.Count == 0)
            {
                throw Error.ArgumentOutOfRange("whens");
            }
            this.Else = @else;
        }

        // Properties
        internal SqlExpression Else
        {
            get
            {
                return this.@else;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((this.@else != null) && !this.@else.ClrType.IsAssignableFrom(value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.@else.ClrType, value.ClrType);
                }
                this.@else = value;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.whens[0].Value.SqlType;
            }
        }

        internal List<SqlWhen> Whens
        {
            get
            {
                return this.whens;
            }
        }

    }
}