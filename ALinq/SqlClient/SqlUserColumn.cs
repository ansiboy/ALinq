using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlUserColumn : SqlSimpleTypeExpression
    {
       
        // Fields
        private bool isRequired;
        private string name;
        private SqlUserQuery query;

        // Methods
        internal SqlUserColumn(Type clrType, IProviderType sqlType, SqlUserQuery query, string name, bool isRequired, Expression source)
            : base(SqlNodeType.UserColumn, clrType, sqlType, source)
        {
            this.Query = query;
            this.name = name;
            this.isRequired = isRequired;
        }

        // Properties
        internal bool IsRequired
        {
            get
            {
                return this.isRequired;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal SqlUserQuery Query
        {
            get
            {
                return this.query;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((this.query != null) && (this.query != value))
                {
                    throw Error.ArgumentWrongValue("value");
                }
                this.query = value;
            }
        }

    }
}