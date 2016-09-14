using System;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal enum InternalExpressionType
    {
        Known = 0x7d0,
        LinkedTable = 0x7d1
    }


    internal sealed class KnownExpression : InternalExpression
    {
        // Fields
        private SqlNode node;

        // Methods
        internal KnownExpression(SqlNode node, Type type)
            : base(InternalExpressionType.Known, type)
        {
            this.node = node;
        }

        // Properties
        internal SqlNode Node
        {
            get { return this.node; }
        }
    }
}