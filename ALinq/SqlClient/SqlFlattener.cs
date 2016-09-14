using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ALinq.SqlClient
{
    internal class SqlFlattener
    {
        // Fields
        private Visitor visitor;

        // Methods
        internal SqlFlattener(SqlFactory sql, SqlColumnizer columnizer)
        {
            this.visitor = new Visitor(sql, columnizer);
        }

        internal SqlNode Flatten(SqlNode node)
        {
            node = this.visitor.Visit(node);
            return node;
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlColumnizer columnizer;
            private bool isTopLevel;
            private Dictionary<SqlColumn, SqlColumn> map = new Dictionary<SqlColumn, SqlColumn>();
            private SqlFactory sql;

            // Methods
            internal Visitor(SqlFactory sql, SqlColumnizer columnizer)
            {
                this.sql = sql;
                this.columnizer = columnizer;
                this.isTopLevel = true;
            }

            private void FlattenGroupBy(IList<SqlExpression> exprs)
            {
                List<SqlExpression> list = new List<SqlExpression>(exprs.Count);
                foreach (SqlExpression expression in exprs)
                {
                    if (TypeSystem.IsSequenceType(expression.ClrType))
                    {
                        throw Error.InvalidGroupByExpressionType(expression.ClrType.Name);
                    }
                    this.FlattenGroupByExpression(list, expression);
                }
                exprs.Clear();
                //exprs.AddRange(list);
                foreach (var expression in list)
                {
                    exprs.Add(expression);
                }
            }

            private void FlattenGroupByExpression(IList<SqlExpression> exprs, SqlExpression expr)
            {
                SqlNew new2 = expr as SqlNew;
                if (new2 != null)
                {
                    foreach (SqlMemberAssign assign in new2.Members)
                    {
                        this.FlattenGroupByExpression(exprs, assign.Expression);
                    }
                    foreach (SqlExpression expression in new2.Args)
                    {
                        this.FlattenGroupByExpression(exprs, expression);
                    }
                }
                else if (expr.NodeType == SqlNodeType.TypeCase)
                {
                    SqlTypeCase @case = (SqlTypeCase)expr;
                    this.FlattenGroupByExpression(exprs, @case.Discriminator);
                    foreach (SqlTypeCaseWhen when in @case.Whens)
                    {
                        this.FlattenGroupByExpression(exprs, when.TypeBinding);
                    }
                }
                else if (expr.NodeType == SqlNodeType.Link)
                {
                    SqlLink link = (SqlLink)expr;
                    if (link.Expansion != null)
                    {
                        this.FlattenGroupByExpression(exprs, link.Expansion);
                    }
                    else
                    {
                        foreach (SqlExpression expression2 in link.KeyExpressions)
                        {
                            this.FlattenGroupByExpression(exprs, expression2);
                        }
                    }
                }
                else if (expr.NodeType == SqlNodeType.OptionalValue)
                {
                    SqlOptionalValue value2 = (SqlOptionalValue)expr;
                    this.FlattenGroupByExpression(exprs, value2.HasValue);
                    this.FlattenGroupByExpression(exprs, value2.Value);
                }
                else if (expr.NodeType == SqlNodeType.OuterJoinedValue)
                {
                    this.FlattenGroupByExpression(exprs, ((SqlUnary)expr).Operand);
                }
                else if (expr.NodeType == SqlNodeType.DiscriminatedType)
                {
                    SqlDiscriminatedType type = (SqlDiscriminatedType)expr;
                    this.FlattenGroupByExpression(exprs, type.Discriminator);
                }
                else
                {
                    if ((expr.NodeType != SqlNodeType.ColumnRef) && (expr.NodeType != SqlNodeType.ExprSet))
                    {
                        if (!expr.SqlType.CanBeColumn)
                        {
                            throw Error.InvalidGroupByExpressionType(expr.SqlType.ToQueryString());
                        }
                        throw Error.InvalidGroupByExpression();
                    }
                    exprs.Add(expr);
                }
            }

            private void FlattenOrderBy(IList<SqlOrderExpression> exprs)
            {
                foreach (SqlOrderExpression expression in exprs)
                {
                    if (!expression.Expression.SqlType.IsOrderable)
                    {
                        if (expression.Expression.SqlType.CanBeColumn)
                        {
                            throw Error.InvalidOrderByExpression(expression.Expression.SqlType.ToQueryString());
                        }
                        throw Error.InvalidOrderByExpression(expression.Expression.ClrType.Name);
                    }
                }
            }

            private SqlExpression FlattenSelection(SqlRow row, bool isInput, SqlExpression selection)
            {
                selection = this.columnizer.ColumnizeSelection(selection);
                return new SelectionFlattener(row, this.map, isInput).VisitExpression(selection);
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlColumn column;
                if (this.map.TryGetValue(cref.Column, out column))
                {
                    return new SqlColumnRef(column);
                }
                return cref;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin)
            {
                base.VisitInsert(sin);
                sin.Expression = this.FlattenSelection(sin.Row, true, sin.Expression);
                return sin;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select = base.VisitSelect(select);
                select.Selection = this.FlattenSelection(select.Row, false, select.Selection);
                if (select.GroupBy.Count > 0)
                {
                    this.FlattenGroupBy(select.GroupBy);
                }
                if (select.OrderBy.Count > 0)
                {
                    this.FlattenOrderBy(select.OrderBy);
                }
                if (!this.isTopLevel)
                {
                    select.Selection = new SqlNop(select.Selection.ClrType, select.Selection.SqlType,
                                                  select.SourceExpression);
                }
                return select;
            }

            internal override SqlSelect VisitSelectCore(SqlSelect select)
            {
                SqlSelect select2;
                bool isTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                try
                {
                    select2 = base.VisitSelectCore(select);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                }
                return select2;
            }

            // Nested Types
            private class SelectionFlattener : SqlVisitor
            {
                // Fields
                private bool isInput;
                private Dictionary<SqlColumn, SqlColumn> map;
                private SqlRow row;

                // Methods
                internal SelectionFlattener(SqlRow row, Dictionary<SqlColumn, SqlColumn> map, bool isInput)
                {
                    this.row = row;
                    this.map = map;
                    this.isInput = isInput;
                }

                private SqlColumn FindColumn(IEnumerable<SqlColumn> columns, SqlColumn col)
                {
                    foreach (SqlColumn column in columns)
                    {
                        if (RefersToColumn(column, col))
                        {
                            return column;
                        }
                    }
                    return null;
                }

                private SqlColumn FindColumnWithExpression(IEnumerable<SqlColumn> columns, SqlExpression expr)
                {
                    foreach (SqlColumn column in columns)
                    {
                        if (column == expr)
                        {
                            return column;
                        }
                        if (SqlComparer.AreEqual(column.Expression, expr))
                        {
                            return column;
                        }
                    }
                    return null;
                }

                private SqlColumnRef MakeFlattenedColumn(SqlExpression expr, string name)
                {
                    //================= HACK:用来修正无法找到列 ==========================
                    //if (expr is SqlColumnRef && ((SqlColumnRef)expr).Column is SqlDynamicColumn)
                    //{
                    //    row.Columns.Add(((SqlColumnRef) expr).Column);
                    //    return (SqlColumnRef) expr;
                    //}
                    //=====================================================================

                    SqlColumn item = !this.isInput ? this.FindColumnWithExpression(this.row.Columns, expr) : null;
                    if (item == null)
                    {


                        item = new SqlColumn(expr.ClrType, expr.SqlType, name, null, expr, expr.SourceExpression);
                        this.row.Columns.Add(item);
                    }
                    return new SqlColumnRef(item);
                }

                internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
                {
                    return cq;
                }

                internal override SqlExpression VisitColumn(SqlColumn col)
                {
                    SqlColumn column = this.FindColumn(this.row.Columns, col);
                    if (((column == null) && (col.Expression != null)) && !this.isInput)
                    {
                        column = this.FindColumnWithExpression(this.row.Columns, col.Expression);
                    }
                    if (column == null)
                    {
                        this.row.Columns.Add(col);
                        column = col;
                    }
                    else if (column != col)
                    {
                        if ((col.Expression.NodeType == SqlNodeType.ExprSet) &&
                            (column.Expression.NodeType != SqlNodeType.ExprSet))
                        {
                            column.Expression = col.Expression;
                        }
                        this.map[col] = column;
                    }
                    return new SqlColumnRef(column);
                }



                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    //===============================================================================

                    SqlColumn col = this.FindColumn(this.row.Columns, cref.Column);
                    if (col == null)
                    {
                        return this.MakeFlattenedColumn(cref, null);
                    }
                    return new SqlColumnRef(col);
                }

                internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
                {
                    return ss;
                }
            }
        }
    }
}