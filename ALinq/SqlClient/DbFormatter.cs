using System.Collections.Generic;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal abstract class DbFormatter
    {
        private readonly SqlFormatter.Visitor visitor;

        internal DbFormatter(SqlProvider sqlProvider)
        {
            visitor = CreateVisitor();
            visitor.SqlIdentifier = sqlProvider.SqlIdentifier;
            visitor.Mode = sqlProvider.Mode;
        }


        internal void SetIdentifier(SqlIdentifier sqlIdentifier)
        {
            visitor.SqlIdentifier = sqlIdentifier;
        }

        public bool ParenthesizeTop
        {
            get { return this.visitor.parenthesizeTop; }
            set { this.visitor.parenthesizeTop = value; }
        }

        // Methods

        internal string Format(SqlNode node)
        {
            return Format(node, false);
        }

        internal string Format(SqlNode node, bool isDebug)
        {
            return this.visitor.Format(node, isDebug);
        }

        internal string[] FormatBlock(SqlBlock block, bool isDebug)
        {
            var list = new List<string>(block.Statements.Count);
            int num = 0;
            int count = block.Statements.Count;
            while (num < count)
            {
                var node = block.Statements[num];
                var select = node as SqlSelect;
                if ((select == null) || !select.DoNotOutput)
                {
                    list.Add(this.Format(node, isDebug));
                }
                num++;
            }
            return list.ToArray();
        }

        //internal List<string> Parameters
        //{
        //    get { return this.visitor.Parameters; }
        //}

        internal abstract SqlFormatter.Visitor CreateVisitor();
    }
}