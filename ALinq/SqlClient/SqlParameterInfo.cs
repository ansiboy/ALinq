using System;

namespace ALinq.SqlClient
{
    internal class SqlParameterInfo
    {
        // Fields

        // Methods
        internal SqlParameterInfo(SqlParameter parameter)
        {
            this.Parameter = parameter;
        }

        internal SqlParameterInfo(SqlParameter parameter, Delegate accessor)
        {
            this.Parameter = parameter;
            this.Accessor = accessor;
        }

        internal SqlParameterInfo(SqlParameter parameter, object value)
        {
            this.Parameter = parameter;
            this.Value = value;
        }

        // Properties
        internal Delegate Accessor { get; private set; }

        internal SqlParameter Parameter
        {
            get;
            private set;
        }

        internal SqlParameterType Type
        {
            get
            {
                if (this.Accessor != null)
                {
                    return SqlParameterType.UserArgument;
                }
                if (this.Parameter.Name == "@ROWCOUNT")
                {
                    return SqlParameterType.PreviousResult;
                }
                return SqlParameterType.Value;
            }
        }

        internal object Value { get; private set; }
    }
}