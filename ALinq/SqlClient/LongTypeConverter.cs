using System;

namespace ALinq.SqlClient
{
    internal class LongTypeConverter
    {
        // Fields
        private Visitor visitor;

        // Methods
        internal LongTypeConverter(SqlFactory sql)
        {
            this.visitor = new Visitor(sql);
        }

        internal SqlNode AddConversions(SqlNode node, SqlNodeAnnotations annotations)
        {
            this.visitor.Annotations = annotations;
            return this.visitor.Visit(node);
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlNodeAnnotations annotations;
            private SqlFactory sql;

            // Methods
            internal Visitor(SqlFactory sql)
            {
                this.sql = sql;
            }

            private void ConvertColumnsToMax(SqlSelect select, out bool changed, out bool containsLongExpressions)
            {
                SqlRow row = select.Row;
                changed = false;
                containsLongExpressions = false;
                foreach (SqlColumn column in row.Columns)
                {
                    bool flag;
                    containsLongExpressions = containsLongExpressions || column.SqlType.IsLargeType;
                    column.Expression = this.ConvertToMax(column.Expression, out flag);
                    changed = changed || flag;
                }
            }

            private SqlExpression ConvertToMax(SqlExpression expr, out bool changed)
            {
                changed = false;
                if (expr.SqlType.IsLargeType)
                {
                    IProviderType bestLargeType = this.sql.TypeProvider.GetBestLargeType(expr.SqlType);
                    changed = true;
                    if (expr.SqlType != bestLargeType)
                    {
                        return this.ConvertToMax(expr, bestLargeType);
                    }
                    changed = false;
                }
                return expr;
            }

            private SqlExpression ConvertToMax(SqlExpression expr, IProviderType newType)
            {
                return SqlFactory.UnaryConvert(expr.ClrType, newType, expr, expr.SourceExpression);
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                if (fc.Name == "LEN")
                {
                    bool flag;
                    fc.Arguments[0] = this.ConvertToMax(fc.Arguments[0], out flag);
                    if (fc.Arguments[0].SqlType.IsLargeType)
                    {
                        this.annotations.Add(fc,
                                             new SqlServerCompatibilityAnnotation(
                                                 Strings.LenOfTextOrNTextNotSupported(fc.SourceExpression),
                                                 new SqlProvider.ProviderMode[] {SqlProvider.ProviderMode.Sql2000}));
                    }
                }
                return base.VisitFunctionCall(fc);
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                if (select.IsDistinct)
                {
                    bool flag;
                    bool flag2;
                    this.ConvertColumnsToMax(select, out flag, out flag2);
                    if (flag2)
                    {
                        this.annotations.Add(select,
                                             new SqlServerCompatibilityAnnotation(
                                                 Strings.TextNTextAndImageCannotOccurInDistinct(select.SourceExpression),
                                                 new SqlProvider.ProviderMode[]
                                                     {SqlProvider.ProviderMode.Sql2000, SqlProvider.ProviderMode.SqlCE}));
                    }
                }
                return base.VisitSelect(select);
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                bool changed = false;
                bool containsLongExpressions = false;
                SqlSelect left = su.Left as SqlSelect;
                if (left != null)
                {
                    this.ConvertColumnsToMax(left, out changed, out containsLongExpressions);
                }
                bool flag3 = false;
                bool flag4 = false;
                SqlSelect right = su.Right as SqlSelect;
                if (right != null)
                {
                    this.ConvertColumnsToMax(right, out flag3, out flag4);
                }
                if (!su.All && (containsLongExpressions || flag4))
                {
                    this.annotations.Add(su,
                                         new SqlServerCompatibilityAnnotation(
                                             Strings.TextNTextAndImageCannotOccurInUnion(su.SourceExpression),
                                             new SqlProvider.ProviderMode[]
                                                 {SqlProvider.ProviderMode.Sql2000, SqlProvider.ProviderMode.SqlCE}));
                }
                return base.VisitUnion(su);
            }

            // Properties
            internal SqlNodeAnnotations Annotations
            {
                set { this.annotations = value; }
            }
        }
    }
}