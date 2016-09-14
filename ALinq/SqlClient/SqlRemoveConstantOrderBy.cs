using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class SqlRemoveConstantOrderBy
    {
        // Methods
        internal static SqlNode Remove(SqlNode node)
        {
            return new Visitor().Visit(node);
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Methods
            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                int index = 0;
                IList<SqlOrderExpression> orderBy = select.OrderBy;
                while (index < orderBy.Count)
                {
                    SqlExpression discriminator = orderBy[index].Expression;
                    while (discriminator.NodeType == SqlNodeType.DiscriminatedType)
                    {
                        discriminator = ((SqlDiscriminatedType) discriminator).Discriminator;
                    }
                    switch (discriminator.NodeType)
                    {
                        case SqlNodeType.Parameter:
                        case SqlNodeType.Value:
                            {
                                orderBy.RemoveAt(index);
                                continue;
                            }
                    }
                    index++;
                }
                return base.VisitSelect(select);
            }
        }
    }
}