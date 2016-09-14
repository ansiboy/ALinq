using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlMethodCall : SqlSimpleTypeExpression
    {
         // Fields
        private readonly List<SqlExpression> arguments;
        private readonly MethodInfo method;
        private SqlExpression obj;

        // Methods
        internal SqlMethodCall(Type clrType, IProviderType sqlType, MethodInfo method, SqlExpression obj, IEnumerable<SqlExpression> args, Expression sourceExpression)
            : base(SqlNodeType.MethodCall, clrType, sqlType, sourceExpression)
        {
            if (method == null)
            {
                throw Error.ArgumentNull("method");
            }
            this.method = method;
            Object = obj;
            arguments = new List<SqlExpression>();
            if (args != null)
            {
                arguments.AddRange(args);
            }
        }

        // Properties
        internal List<SqlExpression> Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        internal MethodInfo Method
        {
            get
            {
                return this.method;
            }
        }

        internal SqlExpression Object
        {
            get
            {
                return this.obj;
            }
            set
            {
                if ((value == null) && !this.method.IsStatic)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((value != null) && !this.method.DeclaringType.IsAssignableFrom(value.ClrType))
                {
                    throw Error.ArgumentWrongType("value", this.method.DeclaringType, value.ClrType);
                }
                this.obj = value;
            }
        }

    }
}