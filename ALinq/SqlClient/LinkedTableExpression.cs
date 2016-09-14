using System;
using ALinq;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal sealed class LinkedTableExpression : InternalExpression
    {
        // Fields
        private SqlLink link;
        private ITable table;

        // Methods
        internal LinkedTableExpression(SqlLink link, ITable table, Type type)
            : base(InternalExpressionType.LinkedTable, type)
        {
            this.link = link;
            this.table = table;
        }

        // Properties
        internal SqlLink Link
        {
            get { return this.link; }
        }

        internal ITable Table
        {
            get { return this.table; }
        }
    }
}