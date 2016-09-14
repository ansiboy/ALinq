using System;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq
{
    internal static class TypeSource
    {
        // Methods
        internal static MetaType GetSourceMetaType(SqlNode node, MetaModel model)
        {
            Visitor visitor = new Visitor();
            visitor.Visit(node);
            Type nonNullableType = TypeSystem.GetNonNullableType(visitor.sourceType);
            return model.GetMetaType(nonNullableType);
        }

        internal static SqlExpression GetTypeSource(SqlExpression expr)
        {
            Visitor visitor = new Visitor();
            visitor.Visit(expr);
            return visitor.sourceExpression;
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            internal SqlExpression sourceExpression;
            internal Type sourceType;
            private UnwrapStack UnwrapSequences;

            // Methods
            internal override SqlNode Visit(SqlNode node)
            {
                if (node == null)
                {
                    return null;
                }
                sourceExpression = node as SqlExpression;
                if (sourceExpression != null)
                {
                    Type clrType = sourceExpression.ClrType;
                    for (UnwrapStack stack = UnwrapSequences; stack != null; stack = stack.Last)
                    {
                        if (stack.Unwrap)
                        {
                            clrType = TypeSystem.GetElementType(clrType);
                        }
                    }
                    sourceType = clrType;
                }
                if ((sourceType != null) && TypeSystem.GetNonNullableType(sourceType).IsValueType)
                {
                    return node;
                }
                if ((sourceType != null) && TypeSystem.HasIEnumerable(sourceType))
                {
                    return node;
                }
                switch (node.NodeType)
                {
                    case SqlNodeType.ClientCase:
                    case SqlNodeType.Convert:
                    case SqlNodeType.DiscriminatedType:
                        return node;

                    case SqlNodeType.MethodCall:
                    case SqlNodeType.Member:
                        return node;

                    case SqlNodeType.Link:
                        sourceType = ((SqlLink) node).RowType.Type;
                        return node;

                    case SqlNodeType.Element:
                    case SqlNodeType.FunctionCall:
                        return node;

                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.SearchedCase:
                        return node;

                    case SqlNodeType.New:
                        return node;

                    case SqlNodeType.Multiset:
                        return node;

                    case SqlNodeType.TypeCase:
                        sourceType = ((SqlTypeCase) node).RowType.Type;
                        return node;

                    case SqlNodeType.Value:
                        {
                            SqlValue value2 = (SqlValue) node;
                            if (value2.Value != null)
                            {
                                sourceType = value2.Value.GetType();
                            }
                            return node;
                        }
                    case SqlNodeType.SimpleCase:
                        return node;

                    case SqlNodeType.Table:
                        sourceType = ((SqlTable) node).RowType.Type;
                        return node;
                }
                return base.Visit(node);
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                if ((UnwrapSequences != null) && UnwrapSequences.Unwrap)
                {
                    UnwrapSequences = new UnwrapStack(UnwrapSequences, false);
                    VisitAlias(aref.Alias);
                    UnwrapSequences = UnwrapSequences.Last;
                    return aref;
                }
                VisitAlias(aref.Alias);
                return aref;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                VisitColumn(cref.Column);
                return cref;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                UnwrapSequences = new UnwrapStack(UnwrapSequences, true);
                VisitExpression(select.Selection);
                UnwrapSequences = UnwrapSequences.Last;
                return select;
            }

            // Nested Types
            private class UnwrapStack
            {
                // Methods
                public UnwrapStack(UnwrapStack last, bool unwrap)
                {
                    Last = last;
                    Unwrap = unwrap;
                }

                // Properties
                public UnwrapStack Last { get; set; }

                public bool Unwrap { get; set; }
            }
        }
    }
}