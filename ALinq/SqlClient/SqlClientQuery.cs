using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ALinq.SqlClient
{
    internal class SqlClientQuery : SqlSimpleTypeExpression
    {
       
        // Fields
        private List<SqlExpression> arguments;
        private int ordinal;
        private List<SqlParameter> parameters;
        private SqlSubSelect query;

        // Methods
        internal SqlClientQuery(SqlSubSelect subquery)
            : base(SqlNodeType.ClientQuery, subquery.ClrType, subquery.SqlType, subquery.SourceExpression)
        {
            this.query = subquery;
            this.arguments = new List<SqlExpression>();
            this.parameters = new List<SqlParameter>();
        }

        // Properties
        internal List<SqlExpression> Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        internal int Ordinal
        {
            get
            {
                return this.ordinal;
            }
            set
            {
                this.ordinal = value;
            }
        }

        internal List<SqlParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal SqlSubSelect Query
        {
            get
            {
                return this.query;
            }
            set
            {
                if ((value == null) || ((this.query != null) && (this.query.ClrType != value.ClrType)))
                {
                    throw Error.ArgumentWrongType(value, this.query.ClrType, value.ClrType);
                }
                this.query = value;
            }
        }

    }
}