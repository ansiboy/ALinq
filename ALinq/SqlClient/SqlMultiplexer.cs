using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ALinq.SqlClient
{



    internal class SqlMultiplexer
    {
        // Fields
        private readonly Visitor visitor;
        private readonly SqlIdentifier sqlIdentity;

        // Methods
        internal SqlMultiplexer(Options options, IEnumerable<SqlParameter> parentParameters, SqlFactory sqlFactory, SqlIdentifier sqlIdentity)
        {
            this.sqlIdentity = sqlIdentity;
            this.visitor = new Visitor(options, parentParameters, sqlFactory, sqlIdentity);
        }

        internal SqlNode Multiplex(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        // Nested Types
        internal enum Options
        {
            None,
            EnableBigJoin
        }

        private class Visitor : SqlVisitor
        {
            // Fields
            private bool canJoin;
            private bool hasBigJoin;
            private bool isTopLevel;
            private readonly Options options;
            private SqlSelect outerSelect;
            private readonly IEnumerable<SqlParameter> parentParameters;
            private readonly SqlFactory sql;
            private readonly SqlIdentifier sqlIdentity;

            // Methods
            internal Visitor(Options options, IEnumerable<SqlParameter> parentParameters,
                             SqlFactory sqlFactory, SqlIdentifier sqlIdentity)
            {
                this.options = options;
                this.sql = sqlFactory;
                this.canJoin = true;
                this.isTopLevel = true;
                this.parentParameters = parentParameters;
                this.sqlIdentity = sqlIdentity;
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                SqlExpression expression;
                bool canJoin = this.canJoin;
                this.canJoin = false;
                try
                {
                    expression = base.VisitClientCase(c);
                }
                finally
                {
                    this.canJoin = canJoin;
                }
                return expression;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                return QueryExtractor.Extract(elem, this.parentParameters, sqlIdentity);
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss)
            {
                SqlExpression expression;
                bool isTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                bool canJoin = this.canJoin;
                this.canJoin = false;
                try
                {
                    expression = base.VisitExists(ss);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                    this.canJoin = canJoin;
                }
                return expression;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                if (((((this.options & SqlMultiplexer.Options.EnableBigJoin) != SqlMultiplexer.Options.None) && !this.hasBigJoin) &&
                        (this.canJoin && this.isTopLevel)) && (((this.outerSelect != null) && !MultisetChecker.HasMultiset(sms.Select.Selection)) &&
                        BigJoinChecker.CanBigJoin(sms.Select)))
                {
                    sms.Select = this.VisitSelect(sms.Select);
                    SqlAlias right = new SqlAlias(sms.Select);
                    SqlJoin join = new SqlJoin(SqlJoinType.OuterApply, this.outerSelect.From, right, null, sms.SourceExpression);
                    this.outerSelect.From = join;
                    this.outerSelect.OrderingType = SqlOrderingType.Always;
                    SqlExpression expression = (SqlExpression)SqlDuplicator.Copy(sms.Select.Selection);
                    SqlSelect node = (SqlSelect)SqlDuplicator.Copy(sms.Select);
                    SqlAlias from = new SqlAlias(node);
                    SqlSelect select = new SqlSelect(this.sql.Unary(SqlNodeType.Count, null, sms.SourceExpression), from, sms.SourceExpression);
                    select.OrderingType = SqlOrderingType.Never;
                    SqlExpression count = this.sql.SubSelect(SqlNodeType.ScalarSubSelect, select);
                    SqlJoinedCollection joineds = new SqlJoinedCollection(sms.ClrType, sms.SqlType, expression, count, sms.SourceExpression);
                    this.hasBigJoin = true;
                    return joineds;
                }
                return QueryExtractor.Extract(sms, this.parentParameters, sqlIdentity);
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                SqlExpression expression;
                bool canJoin = this.canJoin;
                this.canJoin = false;
                try
                {
                    expression = base.VisitOptionalValue(sov);
                }
                finally
                {
                    this.canJoin = canJoin;
                }
                return expression;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                SqlExpression expression;
                bool isTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                bool canJoin = this.canJoin;
                this.canJoin = false;
                try
                {
                    expression = base.VisitScalarSubSelect(ss);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                    this.canJoin = canJoin;
                }
                return expression;
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                SqlExpression expression;
                bool canJoin = this.canJoin;
                this.canJoin = false;
                try
                {
                    expression = base.VisitSearchedCase(c);
                }
                finally
                {
                    this.canJoin = canJoin;
                }
                return expression;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlSelect outerSelect = this.outerSelect;
                this.outerSelect = select;
                this.canJoin &= ((select.GroupBy.Count == 0) && (select.Top == null)) && !select.IsDistinct;
                bool isTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                select.From = this.VisitSource(select.From);
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
                select.Row = (SqlRow)this.Visit(select.Row);
                this.isTopLevel = isTopLevel;
                select.Selection = this.VisitExpression(select.Selection);
                this.isTopLevel = isTopLevel;
                this.outerSelect = outerSelect;
                if (select.IsDistinct && HierarchyChecker.HasHierarchy(select.Selection))
                {
                    select.IsDistinct = false;
                }
                return select;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                SqlExpression expression;
                bool canJoin = this.canJoin;
                this.canJoin = false;
                try
                {
                    expression = base.VisitSimpleCase(c);
                }
                finally
                {
                    this.canJoin = canJoin;
                }
                return expression;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                SqlExpression expression;
                bool canJoin = this.canJoin;
                this.canJoin = false;
                try
                {
                    expression = base.VisitTypeCase(tc);
                }
                finally
                {
                    this.canJoin = canJoin;
                }
                return expression;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                this.canJoin = false;
                return base.VisitUnion(su);
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                this.canJoin = false;
                return base.VisitUserQuery(suq);
            }

            //internal override SqlExpression VisitValue(SqlValue value)
            //{
            //    return base.VisitValue(value);
            //}
        }
    }


    
}