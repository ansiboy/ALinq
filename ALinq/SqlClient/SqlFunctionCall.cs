using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace ALinq.SqlClient
{
    internal class SqlFunctionCall : SqlSimpleTypeExpression
    {
        // Fields

        // Methods
        internal SqlFunctionCall(Type clrType, IProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
            : this(SqlNodeType.FunctionCall, clrType, sqlType, name, args, source)
        {
            Comma = true;
        }

        internal SqlFunctionCall(SqlNodeType nodeType, Type clrType, IProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
            : base(nodeType, clrType, sqlType, source)
        {
            this.Name = name;
            this.Arguments = new MyList(args);
            Comma = true;
            Brackets = true;
        }

        // Properties
        internal MyList Arguments { get; private set; }

        internal string Name { get; private set; }

        internal bool Comma { get; set; }

        internal bool Brackets { get; set; }
    }

    internal class GetItemFunction : SqlFunctionCall
    {
        private SqlFunctionCall source;

        public GetItemFunction(SqlFunctionCall source)
            : base(source.NodeType, source.ClrType, source.SqlType, source.Name, source.Arguments, source.SourceExpression)
        {
            this.source = source;
            Arguments.ForEach(o => ((SqlValue) o).IsClientSpecified = false);
        }

        internal override IProviderType SqlType
        {
            get { return source.SqlType; }
        }
    }

    class MyList : List<SqlExpression>
    {
        public MyList(IEnumerable<SqlExpression> args)
            : base(args)
        {

        }

        public new SqlExpression this[int index]
        {

            get { return base[index]; }
            set { base[index] = value; }

        }

    }

}