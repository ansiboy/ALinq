using System;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlLift : SqlExpression
    {
       // Fields
        internal SqlExpression liftedExpression;

        // Methods
        internal SqlLift(Type type, SqlExpression liftedExpression, Expression sourceExpression)
            : base(SqlNodeType.Lift, type, sourceExpression)
        {
            if (liftedExpression == null)
            {
                throw Error.ArgumentNull("liftedExpression");
            }
            this.liftedExpression = liftedExpression;
        }

        // Properties
        internal SqlExpression Expression
        {
            get
            {
                return this.liftedExpression;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.liftedExpression = value;
            }
        }

        internal override IProviderType SqlType
        {
            get
            {
                return this.liftedExpression.SqlType;
            }
        }

    }
}