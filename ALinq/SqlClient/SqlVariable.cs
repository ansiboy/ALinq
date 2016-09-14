using System;
using System.Diagnostics;
using System.Linq.Expressions;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal class SqlVariable : SqlSimpleTypeExpression
    {
        // Fields
        private string name;

        // Methods
        internal SqlVariable(Type clrType, IProviderType sqlType, string name, Expression sourceExpression)
            : base(SqlNodeType.Variable, clrType, sqlType, sourceExpression)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }
            this.name = name;
        }

        // Properties
        internal string Name
        {
            get
            {
                return this.name;
            }
        }

    }
}