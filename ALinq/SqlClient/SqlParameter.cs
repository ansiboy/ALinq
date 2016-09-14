using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlParameter : SqlSimpleTypeExpression
    {
        
        // Fields

        // Methods
        internal SqlParameter(Type clrType, IProviderType sqlType, string name, Expression sourceExpression)
            : base(SqlNodeType.Parameter, clrType, sqlType, sourceExpression)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }
            if (typeof(Type).IsAssignableFrom(clrType))
            {
                throw Error.ArgumentWrongValue("clrType");
            }
            this.Name = name;
            this.Direction = ParameterDirection.Input;
        }

        // Properties
        internal ParameterDirection Direction { get; set; }

        internal string Name { get; set; }
    }
}