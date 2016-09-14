using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlRowNumber : SqlSimpleTypeExpression
    {

        // Fields
        private List<SqlOrderExpression> orderBy;

        // Methods
        internal SqlRowNumber(Type clrType, IProviderType sqlType, List<SqlOrderExpression> orderByList, Expression sourceExpression)
            : base(SqlNodeType.RowNumber, clrType, sqlType, sourceExpression)
        {
            if (orderByList == null)
            {
                throw Error.ArgumentNull("orderByList");
            }
            this.orderBy = orderByList;
        }

        // Properties
        internal List<SqlOrderExpression> OrderBy
        {
            get
            {
                return this.orderBy;
            }
        }

    }

    //internal class MySqlRowNumber : SqlSimpleTypeExpression
    //{
    //    public MySqlRowNumber(Type clrType, IProviderType sqlType, List<SqlOrderExpression> orderByList, Expression sourceExpression)
    //        : base(SqlNodeType.MyRowNumber, clrType, sqlType, sourceExpression)
    //    {
    //    }
    //}
}