using System.Collections.Generic;
using System.Linq;

namespace ALinq.SqlClient
{
    internal class SqlOuterApplyReducer
    {
        // Methods
        internal static SqlNode Reduce(SqlNode node, SqlFactory factory, SqlNodeAnnotations annotations)
        {
            Visitor visitor = new Visitor(factory, annotations);
            return visitor.Visit(node);
        }

        // Nested Types
        private static class SqlAliasDependencyChecker
        {
            // Methods
            internal static bool IsDependent(SqlNode node, Dictionary<SqlAlias, bool> aliasesToCheck,
                                             Dictionary<SqlExpression, bool> ignoreExpressions)
            {
                Visitor visitor = new Visitor(aliasesToCheck, ignoreExpressions);
                visitor.Visit(node);
                return visitor.hasDependency;
            }

            // Nested Types
            private class Visitor : SqlVisitor
            {
                // Fields
                private Dictionary<SqlAlias, bool> aliasesToCheck;
                internal bool hasDependency;
                private Dictionary<SqlExpression, bool> ignoreExpressions;

                // Methods
                internal Visitor(Dictionary<SqlAlias, bool> aliasesToCheck,
                                 Dictionary<SqlExpression, bool> ignoreExpressions)
                {
                    this.aliasesToCheck = aliasesToCheck;
                    this.ignoreExpressions = ignoreExpressions;
                }

                internal override SqlNode Visit(SqlNode node)
                {
                    SqlExpression key = node as SqlExpression;
                    if (this.hasDependency)
                    {
                        return node;
                    }
                    if ((key != null) && this.ignoreExpressions.ContainsKey(key))
                    {
                        return node;
                    }
                    return base.Visit(node);
                }

                internal override SqlExpression VisitColumn(SqlColumn col)
                {
                    if (col.Expression != null)
                    {
                        this.Visit(col.Expression);
                    }
                    return col;
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    if (this.aliasesToCheck.ContainsKey(cref.Column.Alias))
                    {
                        this.hasDependency = true;
                        return cref;
                    }
                    if (cref.Column.Expression != null)
                    {
                        this.Visit(cref.Column.Expression);
                    }
                    return cref;
                }
            }
        }

        private class SqlAliasesReferenced
        {
            // Fields
            private Dictionary<SqlAlias, bool> aliases;
            private bool referencesAny;
            private Visitor visitor;

            // Methods
            internal SqlAliasesReferenced(Dictionary<SqlAlias, bool> aliases)
            {
                this.aliases = aliases;
                this.visitor = new Visitor(this);
            }

            internal bool ReferencesAny(SqlExpression expression)
            {
                this.referencesAny = false;
                this.visitor.Visit(expression);
                return this.referencesAny;
            }

            // Nested Types
            private class Visitor : SqlVisitor
            {
                // Fields
                private SqlOuterApplyReducer.SqlAliasesReferenced parent;

                // Methods
                internal Visitor(SqlOuterApplyReducer.SqlAliasesReferenced parent)
                {
                    this.parent = parent;
                }

                internal override SqlExpression VisitColumn(SqlColumn col)
                {
                    if (col.Expression != null)
                    {
                        this.Visit(col.Expression);
                    }
                    return col;
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    if (this.parent.aliases.ContainsKey(cref.Column.Alias))
                    {
                        this.parent.referencesAny = true;
                        return cref;
                    }
                    if (cref.Column.Expression != null)
                    {
                        this.Visit(cref.Column.Expression);
                    }
                    return cref;
                }
            }
        }

        private class SqlGatherReferencedColumns
        {
            // Methods
            private SqlGatherReferencedColumns()
            {
            }

            internal static Dictionary<SqlColumn, bool> Gather(SqlNode node, Dictionary<SqlColumn, bool> columns)
            {
                new Visitor(columns).Visit(node);
                return columns;
            }

            // Nested Types
            private class Visitor : SqlVisitor
            {
                // Fields
                private Dictionary<SqlColumn, bool> columns;

                // Methods
                internal Visitor(Dictionary<SqlColumn, bool> columns)
                {
                    this.columns = columns;
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    if (!this.columns.ContainsKey(cref.Column))
                    {
                        this.columns[cref.Column] = true;
                        if (cref.Column.Expression != null)
                        {
                            this.Visit(cref.Column.Expression);
                        }
                    }
                    return cref;
                }
            }
        }

        private static class SqlPredicateLifter
        {
            // Methods
            internal static bool CanLift(SqlSource source, Dictionary<SqlAlias, bool> aliasesForLifting,
                                         Dictionary<SqlExpression, bool> liftedExpressions)
            {
                Visitor visitor = new Visitor(false, aliasesForLifting, liftedExpressions);
                visitor.VisitSource(source);
                return visitor.canLiftAll;
            }

            internal static SqlExpression Lift(SqlSource source, Dictionary<SqlAlias, bool> aliasesForLifting)
            {
                Visitor visitor = new Visitor(true, aliasesForLifting, null);
                visitor.VisitSource(source);
                return visitor.lifted;
            }

            // Nested Types
            private class Visitor : SqlVisitor
            {
                // Fields
                private SqlAggregateChecker aggregateChecker;
                private SqlOuterApplyReducer.SqlAliasesReferenced aliases;
                internal bool canLiftAll;
                private bool doLifting;
                internal SqlExpression lifted;
                private Dictionary<SqlExpression, bool> liftedExpressions;

                // Methods
                internal Visitor(bool doLifting, Dictionary<SqlAlias, bool> aliasesForLifting,
                                 Dictionary<SqlExpression, bool> liftedExpressions)
                {
                    this.doLifting = doLifting;
                    this.aliases = new SqlOuterApplyReducer.SqlAliasesReferenced(aliasesForLifting);
                    this.liftedExpressions = liftedExpressions;
                    this.canLiftAll = true;
                    this.aggregateChecker = new SqlAggregateChecker();
                }

                internal override SqlSelect VisitSelect(SqlSelect select)
                {
                    this.VisitSource(select.From);
                    if (((select.Top != null) || (select.GroupBy.Count > 0)) ||
                        (this.aggregateChecker.HasAggregates(select) || select.IsDistinct))
                    {
                        this.canLiftAll = false;
                    }
                    if ((this.canLiftAll && (select.Where != null)) && this.aliases.ReferencesAny(select.Where))
                    {
                        if (this.liftedExpressions != null)
                        {
                            this.liftedExpressions[select.Where] = true;
                        }
                        if (!this.doLifting)
                        {
                            return select;
                        }
                        if (this.lifted != null)
                        {
                            this.lifted = new SqlBinary(SqlNodeType.And, this.lifted.ClrType, this.lifted.SqlType,
                                                        this.lifted, select.Where);
                        }
                        else
                        {
                            this.lifted = select.Where;
                        }
                        select.Where = null;
                    }
                    return select;
                }
            }
        }

        private static class SqlSelectionLifter
        {
            // Methods
            internal static bool CanLift(SqlSource source, Dictionary<SqlAlias, bool> aliasesForLifting,
                                         Dictionary<SqlExpression, bool> liftedExpressions)
            {
                Visitor visitor = new Visitor(false, aliasesForLifting, liftedExpressions);
                visitor.VisitSource(source);
                return visitor.canLiftAll;
            }

            internal static List<List<SqlColumn>> Lift(SqlSource source, Dictionary<SqlAlias, bool> aliasesForLifting,
                                                       Dictionary<SqlExpression, bool> liftedExpressions)
            {
                Visitor visitor = new Visitor(true, aliasesForLifting, liftedExpressions);
                visitor.VisitSource(source);
                return visitor.lifted;
            }

            // Nested Types
            private class Visitor : SqlVisitor
            {
                // Fields
                private SqlAggregateChecker aggregateChecker;
                private SqlOuterApplyReducer.SqlAliasesReferenced aliases;
                internal bool canLiftAll;
                private bool doLifting;
                private bool hasLifted;
                internal List<List<SqlColumn>> lifted;
                private Dictionary<SqlExpression, bool> liftedExpressions;
                private Dictionary<SqlColumn, bool> referencedColumns;

                // Methods
                internal Visitor(bool doLifting, Dictionary<SqlAlias, bool> aliasesForLifting,
                                 Dictionary<SqlExpression, bool> liftedExpressions)
                {
                    this.doLifting = doLifting;
                    this.aliases = new SqlOuterApplyReducer.SqlAliasesReferenced(aliasesForLifting);
                    this.referencedColumns = new Dictionary<SqlColumn, bool>();
                    this.liftedExpressions = liftedExpressions;
                    this.canLiftAll = true;
                    if (doLifting)
                    {
                        this.lifted = new List<List<SqlColumn>>();
                    }
                    this.aggregateChecker = new SqlAggregateChecker();
                }

                private void ReferenceColumns(SqlExpression expression)
                {
                    if ((expression != null) &&
                        ((this.liftedExpressions == null) || !this.liftedExpressions.ContainsKey(expression)))
                    {
                        SqlOuterApplyReducer.SqlGatherReferencedColumns.Gather(expression, this.referencedColumns);
                    }
                }

                internal override SqlSource VisitJoin(SqlJoin join)
                {
                    this.ReferenceColumns(join.Condition);
                    return base.VisitJoin(join);
                }

                internal override SqlSelect VisitSelect(SqlSelect select)
                {
                    this.ReferenceColumns(select.Where);
                    foreach (SqlOrderExpression expression in select.OrderBy)
                    {
                        this.ReferenceColumns(expression.Expression);
                    }
                    foreach (SqlExpression expression2 in select.GroupBy)
                    {
                        this.ReferenceColumns(expression2);
                    }
                    this.ReferenceColumns(select.Having);
                    List<SqlColumn> item = null;
                    List<SqlColumn> collection = null;
                    foreach (SqlColumn column in select.Row.Columns)
                    {
                        bool flag = this.aliases.ReferencesAny(column.Expression);
                        bool flag2 = this.referencedColumns.ContainsKey(column);
                        if (flag)
                        {
                            if (flag2)
                            {
                                this.canLiftAll = false;
                                this.ReferenceColumns(column);
                            }
                            else
                            {
                                this.hasLifted = true;
                                if (this.doLifting)
                                {
                                    if (item == null)
                                    {
                                        item = new List<SqlColumn>();
                                    }
                                    item.Add(column);
                                }
                            }
                            continue;
                        }
                        if (this.doLifting)
                        {
                            if (collection == null)
                            {
                                collection = new List<SqlColumn>();
                            }
                            collection.Add(column);
                        }
                        this.ReferenceColumns(column);
                    }
                    if (this.canLiftAll)
                    {
                        this.VisitSource(select.From);
                    }
                    if ((((select.Top != null) || (select.GroupBy.Count > 0)) ||
                         (this.aggregateChecker.HasAggregates(select) || select.IsDistinct)) && this.hasLifted)
                    {
                        this.canLiftAll = false;
                    }
                    if (this.doLifting && this.canLiftAll)
                    {
                        select.Row.Columns.Clear();
                        if (collection != null)
                        {
                            //select.Row.Columns.AddRange(collection);
                            foreach (var column in collection)
                            {
                                select.Row.Columns.Add(column);
                            }
                        }
                        if (item != null)
                        {
                            this.lifted.Add(item);
                        }
                    }
                    return select;
                }
            }
        }

        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlNodeAnnotations annotations;
            private SqlFactory factory;

            // Methods
            internal Visitor(SqlFactory factory, SqlNodeAnnotations annotations)
            {
                this.factory = factory;
                this.annotations = annotations;
            }

            private void AnnotateSqlIncompatibility(SqlNode node, params SqlProvider.ProviderMode[] providers)
            {
                this.annotations.Add(node,
                                     new SqlServerCompatibilityAnnotation(
                                         Strings.SourceExpressionAnnotation(node.SourceExpression), providers));
            }

            private SqlJoin GetLeftOuterWithUnreferencedSingletonOnLeft(SqlSource source)
            {
                SqlAlias alias = source as SqlAlias;
                if (alias != null)
                {
                    SqlSelect node = alias.Node as SqlSelect;
                    if ((((node != null) && (node.Where == null)) && ((node.Top == null) && (node.GroupBy.Count == 0))) &&
                        (node.OrderBy.Count == 0))
                    {
                        return this.GetLeftOuterWithUnreferencedSingletonOnLeft(node.From);
                    }
                }
                SqlJoin join = source as SqlJoin;
                if ((join == null) || (join.JoinType != SqlJoinType.LeftOuter))
                {
                    return null;
                }
                if (!this.IsSingletonSelect(join.Left))
                {
                    return null;
                }
                Dictionary<SqlAlias, bool> dictionary = SqlGatherProducedAliases.Gather(join.Left);
                foreach (SqlAlias alias2 in SqlGatherConsumedAliases.Gather(join.Right).Keys)
                {
                    if (dictionary.ContainsKey(alias2))
                    {
                        return null;
                    }
                }
                return join;
            }

            private void GetSelectionsBeforeJoin(SqlSource source, List<List<SqlColumn>> selections)
            {
                if (!(source is SqlJoin))
                {
                    SqlAlias alias = source as SqlAlias;
                    if (alias != null)
                    {
                        SqlSelect node = alias.Node as SqlSelect;
                        if (node != null)
                        {
                            this.GetSelectionsBeforeJoin(node.From, selections);
                            //selections.Add(node.Row.Columns);
                            List<SqlColumn> columns = new List<SqlColumn>(node.Row.Columns);
                            selections.Add(columns);
                        }
                    }
                }
            }

            private bool IsSingletonSelect(SqlSource source)
            {
                SqlAlias alias = source as SqlAlias;
                if (alias == null)
                {
                    return false;
                }
                SqlSelect node = alias.Node as SqlSelect;
                if (node == null)
                {
                    return false;
                }
                if (node.From != null)
                {
                    return false;
                }
                return true;
            }

            private SqlSource PushSourceDown(SqlSource sqlSource, List<SqlColumn> cols)
            {
                SqlSelect node = new SqlSelect(
                    new SqlNop(cols[0].ClrType, cols[0].SqlType, sqlSource.SourceExpression), sqlSource,
                    sqlSource.SourceExpression);
                //node.Row.Columns.AddRange(cols);
                foreach (var col in cols)
                {
                    node.Row.Columns.Add(col);
                }
                return new SqlAlias(node);
            }

            internal override SqlSource VisitSource(SqlSource source)
            {
                source = base.VisitSource(source);
                SqlJoin node = source as SqlJoin;
                if (node != null)
                {
                    if (node.JoinType == SqlJoinType.OuterApply)
                    {
                        Dictionary<SqlAlias, bool> aliasesForLifting = SqlGatherProducedAliases.Gather(node.Left);
                        Dictionary<SqlExpression, bool> liftedExpressions = new Dictionary<SqlExpression, bool>();
                        if (
                            (SqlOuterApplyReducer.SqlPredicateLifter.CanLift(node.Right, aliasesForLifting,
                                                                             liftedExpressions) &&
                             SqlOuterApplyReducer.SqlSelectionLifter.CanLift(node.Right, aliasesForLifting,
                                                                             liftedExpressions)) &&
                            !SqlOuterApplyReducer.SqlAliasDependencyChecker.IsDependent(node.Right, aliasesForLifting,
                                                                                        liftedExpressions))
                        {
                            SqlExpression expression = SqlOuterApplyReducer.SqlPredicateLifter.Lift(node.Right,
                                                                                                    aliasesForLifting);
                            List<List<SqlColumn>> list = SqlOuterApplyReducer.SqlSelectionLifter.Lift(node.Right,
                                                                                                      aliasesForLifting,
                                                                                                      liftedExpressions);
                            node.JoinType = SqlJoinType.LeftOuter;
                            node.Condition = expression;
                            if (list != null)
                            {
                                foreach (List<SqlColumn> list2 in list)
                                {
                                    source = this.PushSourceDown(source, list2);
                                }
                            }
                        }
                        else
                        {
                            this.AnnotateSqlIncompatibility(node,
                                                            new SqlProvider.ProviderMode[]
                                                                {SqlProvider.ProviderMode.Sql2000});
                        }
                    }
                    else if (node.JoinType == SqlJoinType.CrossApply)
                    {
                        SqlJoin leftOuterWithUnreferencedSingletonOnLeft =
                            this.GetLeftOuterWithUnreferencedSingletonOnLeft(node.Right);
                        if (leftOuterWithUnreferencedSingletonOnLeft != null)
                        {
                            Dictionary<SqlAlias, bool> dictionary3 = SqlGatherProducedAliases.Gather(node.Left);
                            Dictionary<SqlExpression, bool> dictionary4 = new Dictionary<SqlExpression, bool>();
                            if (
                                (SqlOuterApplyReducer.SqlPredicateLifter.CanLift(
                                     leftOuterWithUnreferencedSingletonOnLeft.Right, dictionary3, dictionary4) &&
                                 SqlOuterApplyReducer.SqlSelectionLifter.CanLift(
                                     leftOuterWithUnreferencedSingletonOnLeft.Right, dictionary3, dictionary4)) &&
                                !SqlOuterApplyReducer.SqlAliasDependencyChecker.IsDependent(
                                     leftOuterWithUnreferencedSingletonOnLeft.Right, dictionary3, dictionary4))
                            {
                                SqlExpression right =
                                    SqlOuterApplyReducer.SqlPredicateLifter.Lift(
                                        leftOuterWithUnreferencedSingletonOnLeft.Right, dictionary3);
                                List<List<SqlColumn>> selections =
                                    SqlOuterApplyReducer.SqlSelectionLifter.Lift(
                                        leftOuterWithUnreferencedSingletonOnLeft.Right, dictionary3, dictionary4);
                                this.GetSelectionsBeforeJoin(node.Right, selections);
                                foreach (
                                    List<SqlColumn> list4 in
                                        selections.Where<List<SqlColumn>>(
                                            delegate(List<SqlColumn> s) { return s.Count > 0; }))
                                {
                                    source = this.PushSourceDown(source, list4);
                                }
                                node.JoinType = SqlJoinType.LeftOuter;
                                node.Condition =
                                    this.factory.AndAccumulate(leftOuterWithUnreferencedSingletonOnLeft.Condition, right);
                                node.Right = leftOuterWithUnreferencedSingletonOnLeft.Right;
                            }
                            else
                            {
                                this.AnnotateSqlIncompatibility(node,
                                                                new SqlProvider.ProviderMode[]
                                                                    {SqlProvider.ProviderMode.Sql2000});
                            }
                        }
                    }
                    while (node.JoinType == SqlJoinType.LeftOuter)
                    {
                        SqlJoin join3 = this.GetLeftOuterWithUnreferencedSingletonOnLeft(node.Left);
                        if (join3 == null)
                        {
                            return source;
                        }
                        List<List<SqlColumn>> list5 = new List<List<SqlColumn>>();
                        this.GetSelectionsBeforeJoin(node.Left, list5);
                        foreach (List<SqlColumn> list6 in list5)
                        {
                            source = this.PushSourceDown(source, list6);
                        }
                        SqlSource source2 = node.Right;
                        SqlExpression condition = node.Condition;
                        node.Left = join3.Left;
                        node.Right = join3;
                        node.Condition = join3.Condition;
                        join3.Left = join3.Right;
                        join3.Right = source2;
                        join3.Condition = condition;
                    }
                }
                return source;
            }
        }
    }
}