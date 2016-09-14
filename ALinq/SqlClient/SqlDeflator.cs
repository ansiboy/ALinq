using System;
using System.Collections.Generic;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlDeflator
    {
        // Fields
        private SqlAliasDeflator aDeflator = new SqlAliasDeflator();
        private SqlColumnDeflator cDeflator = new SqlColumnDeflator();
        private SqlDuplicateColumnDeflator dupColumnDeflator = new SqlDuplicateColumnDeflator();
        private SqlTopSelectDeflator tsDeflator = new SqlTopSelectDeflator();
        private SqlValueDeflator vDeflator = new SqlValueDeflator();

        // Methods
        internal SqlDeflator()
        {
        }

        internal SqlNode Deflate(SqlNode node)
        {
            node = this.vDeflator.Visit(node);
            node = this.cDeflator.Visit(node);
            node = this.aDeflator.Visit(node);
            node = this.tsDeflator.Visit(node);
            node = this.dupColumnDeflator.Visit(node);
            return node;
        }

        // Nested Types
        private class SqlAliasDeflator : SqlVisitor
        {
            // Fields
            private Dictionary<SqlAlias, SqlAlias> removedMap = new Dictionary<SqlAlias, SqlAlias>();

            // Methods
            internal SqlAliasDeflator()
            {
            }

            private bool HasEmptySource(SqlSource node)
            {
                SqlAlias alias = node as SqlAlias;
                if (alias == null)
                {
                    return false;
                }
                SqlSelect select = alias.Node as SqlSelect;
                if (select == null)
                {
                    return false;
                }
                return (((((select.Row.Columns.Count == 0) && (select.From == null)) && ((select.Where == null) && (select.GroupBy.Count == 0))) && (select.Having == null)) && (select.OrderBy.Count == 0));
            }

            private bool HasTrivialProjection(SqlSelect select)
            {
                foreach (SqlColumn column in select.Row.Columns)
                {
                    if ((column.Expression != null) && (column.Expression.NodeType != SqlNodeType.ColumnRef))
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool HasTrivialSource(SqlSource node)
            {
                SqlJoin join = node as SqlJoin;
                if (join == null)
                {
                    return (node is SqlAlias);
                }
                return (this.HasTrivialSource(join.Left) && this.HasTrivialSource(join.Right));
            }

            private bool IsTrivialSelect(SqlSelect select)
            {
                if ((((select.OrderBy.Count != 0) || (select.GroupBy.Count != 0)) || ((select.Having != null) || (select.Top != null))) || (select.IsDistinct || (select.Where != null)))
                {
                    return false;
                }
                return (this.HasTrivialSource(select.From) && this.HasTrivialProjection(select));
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                SqlAlias alias2;
                SqlAlias key = aref.Alias;
                if (this.removedMap.TryGetValue(key, out alias2))
                {
                    throw Error.InvalidReferenceToRemovedAliasDuringDeflation();
                }
                return aref;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if ((cref.Column.Alias == null) || !this.removedMap.ContainsKey(cref.Column.Alias))
                {
                    return cref;
                }

                SqlColumnRef expression;

                //if (cref.Column is SqlDynamicColumn)
                //    expression = cref;
                //else
                expression = cref.Column.Expression as SqlColumnRef;

                if ((expression != null) && (expression.ClrType != cref.ClrType))
                {
                    expression.SetClrType(cref.ClrType);
                    return this.VisitColumnRef(expression);
                }
                return expression;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                base.VisitJoin(join);
                switch (join.JoinType)
                {
                    case SqlJoinType.Cross:
                    case SqlJoinType.Inner:
                        return join;

                    case SqlJoinType.LeftOuter:
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        {
                            if (!this.HasEmptySource(join.Right))
                            {
                                return join;
                            }
                            SqlAlias right = (SqlAlias)join.Right;
                            this.removedMap[right] = right;
                            return join.Left;
                        }
                }
                return join;
            }

            internal override SqlSource VisitSource(SqlSource node)
            {
                node = (SqlSource)this.Visit(node);
                SqlAlias alias = node as SqlAlias;
                if (alias != null)
                {
                    SqlSelect select = alias.Node as SqlSelect;
                    if ((select != null) && this.IsTrivialSelect(select))
                    {
                        this.removedMap[alias] = alias;
                        node = select.From;
                    }
                }
                return node;
            }
        }

        private class SqlColumnDeflator : SqlVisitor
        {
            // Fields
            private SqlAggregateChecker aggregateChecker = new SqlAggregateChecker();
            private bool forceReferenceAll;
            private bool isTopLevel = true;
            private Dictionary<SqlNode, SqlNode> referenceMap = new Dictionary<SqlNode, SqlNode>();

            // Methods
            internal SqlColumnDeflator()
            {
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                this.referenceMap[cref.Column] = cref.Column;
                return cref;
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss)
            {
                SqlExpression expression;
                bool isTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                try
                {
                    expression = base.VisitExists(ss);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                }
                return expression;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                join.Condition = this.VisitExpression(join.Condition);
                join.Right = this.VisitSource(join.Right);
                join.Left = this.VisitSource(join.Left);
                return join;
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                int num = 0;
                int count = link.KeyExpressions.Count;
                while (num < count)
                {
                    link.KeyExpressions[num] = this.VisitExpression(link.KeyExpressions[num]);
                    num++;
                }
                return link;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                SqlExpression expression;
                bool isTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                bool forceReferenceAll = this.forceReferenceAll;
                this.forceReferenceAll = true;
                try
                {
                    expression = base.VisitScalarSubSelect(ss);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                    this.forceReferenceAll = forceReferenceAll;
                }
                return expression;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                bool forceReferenceAll = this.forceReferenceAll;
                this.forceReferenceAll = false;
                bool isTopLevel = this.isTopLevel;
                try
                {
                    if (this.isTopLevel)
                    {
                        select.Selection = this.VisitExpression(select.Selection);
                    }
                    this.isTopLevel = false;
                    for (int i = select.Row.Columns.Count - 1; i >= 0; i--)
                    {
                        SqlColumn key = select.Row.Columns[i];
                        if (((!forceReferenceAll && !this.referenceMap.ContainsKey(key)) && !select.IsDistinct) &&
                            ((select.GroupBy.Count != 0) || !this.aggregateChecker.HasAggregates(key.Expression)))
                        {
                            select.Row.Columns.RemoveAt(i);
                        }
                        else
                        {
                            this.VisitExpression(key.Expression);
                        }
                    }
                    select.Top = this.VisitExpression(select.Top);
                    for (int j = select.OrderBy.Count - 1; j >= 0; j--)
                    {
                        select.OrderBy[j].Expression = this.VisitExpression(select.OrderBy[j].Expression);
                    }
                    select.Having = this.VisitExpression(select.Having);
                    for (int k = select.GroupBy.Count - 1; k >= 0; k--)
                    {
                        select.GroupBy[k] = this.VisitExpression(select.GroupBy[k]);
                    }
                    select.Where = this.VisitExpression(select.Where);
                    select.From = this.VisitSource(select.From);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                    this.forceReferenceAll = forceReferenceAll;
                }
                return select;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                bool forceReferenceAll = this.forceReferenceAll;
                this.forceReferenceAll = true;
                su.Left = this.Visit(su.Left);
                su.Right = this.Visit(su.Right);
                this.forceReferenceAll = forceReferenceAll;
                return su;
            }
        }

        private class SqlColumnEqualizer : SqlVisitor
        {
            // Fields
            private Dictionary<SqlColumn, SqlColumn> map;

            // Methods
            internal SqlColumnEqualizer()
            {
            }

            internal bool AreEquivalent(SqlExpression e1, SqlExpression e2)
            {
                SqlColumn column3;
                if (SqlComparer.AreEqual(e1, e2))
                {
                    return true;
                }
                SqlColumnRef ref2 = e1 as SqlColumnRef;
                SqlColumnRef ref3 = e2 as SqlColumnRef;
                if ((ref2 == null) || (ref3 == null))
                {
                    return false;
                }
                SqlColumn rootColumn = ref2.GetRootColumn();
                SqlColumn column2 = ref3.GetRootColumn();
                return (this.map.TryGetValue(rootColumn, out column3) && (column3 == column2));
            }

            internal void BuildEqivalenceMap(SqlSource scope)
            {
                this.map = new Dictionary<SqlColumn, SqlColumn>();
                this.Visit(scope);
            }

            private void CheckJoinCondition(SqlExpression expr)
            {
                switch (expr.NodeType)
                {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        {
                            SqlBinary binary2 = (SqlBinary)expr;
                            SqlColumnRef left = binary2.Left as SqlColumnRef;
                            SqlColumnRef right = binary2.Right as SqlColumnRef;
                            if ((left != null) && (right != null))
                            {
                                SqlColumn rootColumn = left.GetRootColumn();
                                SqlColumn column2 = right.GetRootColumn();
                                this.map[rootColumn] = column2;
                                this.map[column2] = rootColumn;
                            }
                            return;
                        }
                    case SqlNodeType.And:
                        {
                            SqlBinary binary = (SqlBinary)expr;
                            this.CheckJoinCondition(binary.Left);
                            this.CheckJoinCondition(binary.Right);
                            return;
                        }
                }
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                base.VisitJoin(join);
                if (join.Condition != null)
                {
                    this.CheckJoinCondition(join.Condition);
                }
                return join;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                base.VisitSelect(select);
                if (select.Where != null)
                {
                    this.CheckJoinCondition(select.Where);
                }
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return ss;
            }
        }

        private class SqlDuplicateColumnDeflator : SqlVisitor
        {
            // Fields
            private SqlDeflator.SqlColumnEqualizer equalizer = new SqlDeflator.SqlColumnEqualizer();

            // Methods
            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select.From = this.VisitSource(select.From);
                select.Where = this.VisitExpression(select.Where);
                int num = 0;
                int count = select.GroupBy.Count;
                while (num < count)
                {
                    select.GroupBy[num] = this.VisitExpression(select.GroupBy[num]);
                    num++;
                }
                for (int i = select.GroupBy.Count - 1; i >= 0; i--)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (SqlComparer.AreEqual(select.GroupBy[i], select.GroupBy[j]))
                        {
                            select.GroupBy.RemoveAt(i);
                            break;
                        }
                    }
                }
                select.Having = this.VisitExpression(select.Having);
                int num5 = 0;
                int num6 = select.OrderBy.Count;
                while (num5 < num6)
                {
                    select.OrderBy[num5].Expression = this.VisitExpression(select.OrderBy[num5].Expression);
                    num5++;
                }
                if (select.OrderBy.Count > 0)
                {
                    this.equalizer.BuildEqivalenceMap(select.From);
                    for (int k = select.OrderBy.Count - 1; k >= 0; k--)
                    {
                        for (int m = k - 1; m >= 0; m--)
                        {
                            if (this.equalizer.AreEquivalent(select.OrderBy[k].Expression, select.OrderBy[m].Expression))
                            {
                                select.OrderBy.RemoveAt(k);
                                break;
                            }
                        }
                    }
                }
                select.Top = this.VisitExpression(select.Top);
                select.Row = (SqlRow)this.Visit(select.Row);
                select.Selection = this.VisitExpression(select.Selection);
                return select;
            }
        }

        private class SqlTopSelectDeflator : SqlVisitor
        {
            // Methods
            private bool HasTrivialProjection(SqlSelect select)
            {
                foreach (SqlColumn column in select.Row.Columns)
                {
                    if ((column.Expression != null) && (column.Expression.NodeType != SqlNodeType.ColumnRef))
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool HasTrivialSource(SqlSource node)
            {
                SqlAlias alias = node as SqlAlias;
                if (alias == null)
                {
                    return false;
                }
                return (alias.Node is SqlSelect);
            }

            private bool IsTrivialSelect(SqlSelect select)
            {
                if ((((select.OrderBy.Count != 0) || (select.GroupBy.Count != 0)) || ((select.Having != null) || (select.Top != null))) || (select.IsDistinct || (select.Where != null)))
                {
                    return false;
                }
                return (this.HasTrivialSource(select.From) && this.HasTrivialProjection(select));
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                if (!this.IsTrivialSelect(select))
                {
                    return select;
                }
                SqlSelect node = (SqlSelect)((SqlAlias)select.From).Node;
                Dictionary<SqlColumn, SqlColumnRef> map = new Dictionary<SqlColumn, SqlColumnRef>();
                foreach (SqlColumn column in select.Row.Columns)
                {
                    SqlColumnRef expression;

                    //if (column is SqlDynamicColumn)
                    //{
                    //    expression = new SqlColumnRef(column);
                    //}
                    //else
                    //{
                        expression = (SqlColumnRef)column.Expression;
                    //}

                    map.Add(column, expression);
                    if (!string.IsNullOrEmpty(column.Name))
                    {
                        expression.Column.Name = column.Name;
                    }
                }
                node.Selection = new ColumnMapper(map).VisitExpression(select.Selection);
                return node;
            }

            // Nested Types
            private class ColumnMapper : SqlVisitor
            {
                // Fields
                private Dictionary<SqlColumn, SqlColumnRef> map;

                // Methods
                internal ColumnMapper(Dictionary<SqlColumn, SqlColumnRef> map)
                {
                    this.map = map;
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    SqlColumnRef ref2;
                    if (this.map.TryGetValue(cref.Column, out ref2))
                    {
                        return ref2;
                    }
                    return cref;
                }
            }
        }

        private class SqlValueDeflator : SqlVisitor
        {
            // Fields
            private bool isTopLevel = true;
            private SelectionDeflator sDeflator = new SelectionDeflator();

            // Methods
            internal SqlValueDeflator()
            {
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                if (this.isTopLevel)
                {
                    select.Selection = this.sDeflator.VisitExpression(select.Selection);
                }
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                SqlExpression expression;
                bool isTopLevel = this.isTopLevel;
                try
                {
                    expression = base.VisitSubSelect(ss);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                }
                return expression;
            }

            // Nested Types
            private class SelectionDeflator : SqlVisitor
            {
                // Methods
                private SqlValue GetLiteralValue(SqlExpression expr)
                {
                    while ((expr != null) && (expr.NodeType == SqlNodeType.ColumnRef))
                    {
                        expr = ((SqlColumnRef)expr).Column.Expression;
                    }
                    return (expr as SqlValue);
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    SqlExpression literalValue = this.GetLiteralValue(cref);
                    if (literalValue != null)
                    {
                        return literalValue;
                    }
                    return cref;
                }
            }
        }
    }


    
}