namespace ALinq.SqlClient
{
    internal class SqlResolver
    {
        // Fields
        private Visitor visitor = new Visitor();

        // Methods
        internal SqlResolver()
        {
        }

        private static string GetColumnName(SqlColumn c)
        {
            return c.Name;
        }

        internal SqlNode Resolve(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        // Nested Types
        internal class SqlBubbler : SqlVisitor
        {
            // Fields
            private SqlColumn found;
            private SqlColumn match;

            // Methods
            internal SqlBubbler()
            {
            }

            internal SqlColumn BubbleUp(SqlColumn col, SqlNode source)
            {
                this.match = this.GetOriginatingColumn(col);
                this.found = null;
                this.Visit(source);
                return this.found;
            }

            private void ForceLocal(SqlRow row, string name)
            {
                bool flag = false;
                foreach (SqlColumn column in row.Columns)
                {
                    if (RefersToColumn(column, this.found))
                    {
                        this.found = column;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    SqlColumn item = new SqlColumn(this.found.ClrType, this.found.SqlType, name, this.found.MetaMember,
                                                   new SqlColumnRef(this.found), row.SourceExpression);
                    row.Columns.Add(item);
                    this.found = item;
                }
            }

            internal SqlColumn GetOriginatingColumn(SqlColumn col)
            {
                SqlColumnRef expression = col.Expression as SqlColumnRef;
                if (expression != null)
                {
                    return this.GetOriginatingColumn(expression.Column);
                }
                return col;
            }

            private bool IsFoundInGroup(SqlSelect select)
            {
                foreach (SqlExpression expression in select.GroupBy)
                {
                    if (RefersToColumn(expression, this.found) || RefersToColumn(expression, this.match))
                    {
                        return true;
                    }
                }
                return false;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                switch (join.JoinType)
                {
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        this.Visit(join.Left);
                        if (this.found == null)
                        {
                            this.Visit(join.Right);
                        }
                        return join;
                }
                this.Visit(join.Left);
                this.Visit(join.Right);
                return join;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                foreach (SqlColumn column in row.Columns)
                {
                    if (RefersToColumn(column, this.match))
                    {
                        if (this.found != null)
                        {
                            throw Error.ColumnIsDefinedInMultiplePlaces(SqlResolver.GetColumnName(this.match));
                        }
                        this.found = column;
                        return row;
                    }
                }
                return row;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                this.Visit(select.Row);
                if (this.found == null)
                {
                    this.Visit(select.From);
                    if (this.found == null)
                    {
                        return select;
                    }
                    if (select.IsDistinct && !this.match.IsConstantColumn)
                    {
                        throw Error.ColumnIsNotAccessibleThroughDistinct(SqlResolver.GetColumnName(this.match));
                    }
                    if ((select.GroupBy.Count != 0) && !this.IsFoundInGroup(select))
                    {
                        throw Error.ColumnIsNotAccessibleThroughGroupBy(SqlResolver.GetColumnName(this.match));
                    }
                    this.ForceLocal(select.Row, this.found.Name);
                }
                return select;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                foreach (SqlColumn column in tab.Columns)
                {
                    if (column == this.match)
                    {
                        if (this.found != null)
                        {
                            throw Error.ColumnIsDefinedInMultiplePlaces(SqlResolver.GetColumnName(this.match));
                        }
                        this.found = column;
                        return tab;
                    }
                }
                return tab;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                foreach (SqlColumn column in fc.Columns)
                {
                    if (column == this.match)
                    {
                        if (this.found != null)
                        {
                            throw Error.ColumnIsDefinedInMultiplePlaces(SqlResolver.GetColumnName(this.match));
                        }
                        this.found = column;
                        return fc;
                    }
                }
                return fc;
            }
        }

        internal class SqlScopedVisitor : SqlVisitor
        {
            // Fields
            internal Scope CurrentScope = new Scope(null, null);

            // Methods
            internal SqlScopedVisitor()
            {
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                Scope currentScope = this.CurrentScope;
                this.CurrentScope = new Scope(sd, this.CurrentScope.ContainingScope);
                base.VisitDelete(sd);
                this.CurrentScope = currentScope;
                return sd;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin)
            {
                Scope currentScope = this.CurrentScope;
                this.CurrentScope = new Scope(sin, this.CurrentScope.ContainingScope);
                base.VisitInsert(sin);
                this.CurrentScope = currentScope;
                return sin;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                Scope currentScope = this.CurrentScope;
                switch (join.JoinType)
                {
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        {
                            this.Visit(join.Left);
                            Scope containing = new Scope(join.Left, this.CurrentScope.ContainingScope);
                            this.CurrentScope = new Scope(null, containing);
                            this.Visit(join.Right);
                            Scope scope3 = new Scope(join.Right, containing);
                            this.CurrentScope = new Scope(null, scope3);
                            this.Visit(join.Condition);
                            break;
                        }
                    default:
                        this.Visit(join.Left);
                        this.Visit(join.Right);
                        this.CurrentScope = new Scope(null,
                                                      new Scope(join.Right,
                                                                new Scope(join.Left, this.CurrentScope.ContainingScope)));
                        this.Visit(join.Condition);
                        break;
                }
                this.CurrentScope = currentScope;
                return join;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select.From = (SqlSource) this.Visit(select.From);
                Scope currentScope = this.CurrentScope;
                this.CurrentScope = new Scope(select.From, this.CurrentScope.ContainingScope);
                select.Where = this.VisitExpression(select.Where);
                int num = 0;
                int count = select.GroupBy.Count;
                while (num < count)
                {
                    select.GroupBy[num] = this.VisitExpression(select.GroupBy[num]);
                    num++;
                }
                select.Having = this.VisitExpression(select.Having);
                int num3 = 0;
                int num4 = select.OrderBy.Count;
                while (num3 < num4)
                {
                    select.OrderBy[num3].Expression = this.VisitExpression(select.OrderBy[num3].Expression);
                    num3++;
                }
                select.Top = this.VisitExpression(select.Top);
                select.Row = (SqlRow) this.Visit(select.Row);
                this.CurrentScope = new Scope(select, this.CurrentScope.ContainingScope);
                select.Selection = this.VisitExpression(select.Selection);
                this.CurrentScope = currentScope;
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                Scope currentScope = this.CurrentScope;
                this.CurrentScope = new Scope(null, this.CurrentScope);
                base.VisitSubSelect(ss);
                this.CurrentScope = currentScope;
                return ss;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate sup)
            {
                Scope currentScope = this.CurrentScope;
                this.CurrentScope = new Scope(sup.Select, this.CurrentScope.ContainingScope);
                base.VisitUpdate(sup);
                this.CurrentScope = currentScope;
                return sup;
            }

            // Nested Types
            internal class Scope
            {
                // Fields
                private SqlResolver.SqlScopedVisitor.Scope containing;
                private SqlNode source;

                // Methods
                internal Scope(SqlNode source, SqlResolver.SqlScopedVisitor.Scope containing)
                {
                    this.source = source;
                    this.containing = containing;
                }

                // Properties
                internal SqlResolver.SqlScopedVisitor.Scope ContainingScope
                {
                    get { return this.containing; }
                }

                internal SqlNode Source
                {
                    get { return this.source; }
                }
            }
        }

        private class Visitor : SqlResolver.SqlScopedVisitor
        {
            // Fields
            private SqlResolver.SqlBubbler bubbler = new SqlResolver.SqlBubbler();

            // Methods
            internal Visitor()
            {
            }

            private SqlColumnRef BubbleUp(SqlColumnRef cref)
            {
                for (SqlResolver.SqlScopedVisitor.Scope scope = base.CurrentScope;
                     scope != null;
                     scope = scope.ContainingScope)
                {
                    if (scope.Source != null)
                    {
                        SqlColumn col = this.bubbler.BubbleUp(cref.Column, scope.Source);
                        if (col != null)
                        {
                            if (col != cref.Column)
                            {
                                return new SqlColumnRef(col);
                            }
                            return cref;
                        }
                    }
                }
                return null;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlColumnRef ref2 = this.BubbleUp(cref);
                if (ref2 == null)
                {
                    throw Error.ColumnReferencedIsNotInScope(SqlResolver.GetColumnName(cref.Column));
                }
                return ref2;
            }
        }
    }
}