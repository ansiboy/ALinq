namespace ALinq.SqlClient
{
    internal class SqlAlias : SqlSource
    {
        // Fields
        private SqlNode node;

        // Methods
        internal SqlAlias(SqlNode node)
            : base(SqlNodeType.Alias, node.SourceExpression)
        {
            this.Node = node;
        }

        // Properties
        internal string Name { get; set; }

        internal SqlNode Node
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.node;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                if ((!(value is SqlExpression) && !(value is SqlSelect)) && (!(value is SqlTable) && !(value is SqlUnion)))
                {
                    throw Error.UnexpectedNode(value.NodeType);
                }
                this.node = value;
            }
        }

        public override string Text
        {
            get
            {
                return string.Format("{0}({1})", NodeType, Node.Text);
            }
        }

    }
}