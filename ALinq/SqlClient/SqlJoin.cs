using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlJoin : SqlSource
    {
       
        // Fields
        private SqlExpression condition;
        private SqlJoinType joinType;
        private SqlSource left;
        private SqlSource right;

        // Methods
        internal SqlJoin(SqlJoinType type, SqlSource left, SqlSource right, SqlExpression cond, Expression sourceExpression)
            : base(SqlNodeType.Join, sourceExpression)
        {
            this.JoinType = type;
            this.Left = left;
            this.Right = right;
            this.Condition = cond;
        }

        // Properties
        internal SqlExpression Condition
        {
            get
            {
                return this.condition;
            }
            set
            {
                this.condition = value;
            }
        }

        internal SqlJoinType JoinType
        {
            get
            {
                return this.joinType;
            }
            set
            {
                this.joinType = value;
            }
        }

        internal SqlSource Left
        {
            get
            {
                return this.left;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.left = value;
            }
        }

        internal SqlSource Right
        {
            get
            {
                return this.right;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.right = value;
            }
        }

    }
}