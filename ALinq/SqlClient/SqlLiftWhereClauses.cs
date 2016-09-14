using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlLiftWhereClauses
    {
        // Methods
        internal static SqlNode Lift(SqlNode node, ITypeSystemProvider typeProvider, MetaModel model)
        {
            return new Lifter(typeProvider, model).Visit(node);
        }

        // Nested Types
        private class Lifter : SqlVisitor
        {
            // Fields
            private SqlAggregateChecker aggregateChecker;
            private Scope current;
            private SqlRowNumberChecker rowNumberChecker;
            private SqlFactory sql;

            // Methods
            internal Lifter(ITypeSystemProvider typeProvider, MetaModel model)
            {
                this.sql = new SqlFactory(typeProvider, model);
                this.aggregateChecker = new SqlAggregateChecker();
                this.rowNumberChecker = new SqlRowNumberChecker();
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                Scope current = this.current;
                this.current = null;
                SqlExpression expression = base.VisitClientQuery(cq);
                this.current = current;
                return expression;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                SqlSource source;
                Scope current = this.current;
                try
                {
                    switch (join.JoinType)
                    {
                        case SqlJoinType.Cross:
                        case SqlJoinType.Inner:
                        case SqlJoinType.CrossApply:
                            return base.VisitJoin(join);

                        case SqlJoinType.LeftOuter:
                        case SqlJoinType.OuterApply:
                            join.Left = this.VisitSource(join.Left);
                            this.current = null;
                            join.Right = this.VisitSource(join.Right);
                            join.Condition = this.VisitExpression(join.Condition);
                            return join;
                    }
                    this.current = null;
                    source = base.VisitJoin(join);
                }
                finally
                {
                    this.current = current;
                }
                return source;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                Scope current = this.current;
                this.current = new Scope(select.Where, this.current);
                SqlSelect select2 = base.VisitSelect(select);
                bool flag = ((select.IsDistinct || (select.GroupBy.Count > 0)) ||
                             (this.aggregateChecker.HasAggregates(select) || (select.Top != null))) ||
                            this.rowNumberChecker.HasRowNumber(select);
                if (this.current != null)
                {
                    if ((this.current.Parent != null) && !flag)
                    {
                        this.current.Parent.Where = this.sql.AndAccumulate(this.current.Parent.Where, this.current.Where);
                        this.current.Where = null;
                    }
                    select.Where = this.current.Where;
                }
                this.current = current;
                return select2;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                Scope current = this.current;
                this.current = null;
                SqlExpression expression = base.VisitSubSelect(ss);
                this.current = current;
                return expression;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                Scope current = this.current;
                this.current = null;
                SqlNode node = base.VisitUnion(su);
                this.current = current;
                return node;
            }

            // Nested Types
            private class Scope
            {
                // Fields
                internal SqlLiftWhereClauses.Lifter.Scope Parent;
                internal SqlExpression Where;

                // Methods
                internal Scope(SqlExpression where, SqlLiftWhereClauses.Lifter.Scope parent)
                {
                    this.Where = where;
                    this.Parent = parent;
                }
            }
        }
    }
}