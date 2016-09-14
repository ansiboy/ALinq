using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ALinq.SqlClient;
using NpgsqlTypes;

namespace ALinq.PostgreSQL
{
    class PgsqlFormatter : DbFormatter
    {
        public PgsqlFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }

        private class Visitor : SqlFormatter.Visitor
        {
            protected override void WriteName(string name)
            {
                if (SqlIdentifier.NeedToQuote(name))
                    sb.Append(SqlIdentifier.QuoteCompoundIdentifier(name));
                else
                    sb.Append(name);
            }

            internal override string GetBoolValue(bool value)
            {
                return value ? "true" : "false";
            }

            protected override SqlExpression TranslateConverter(SqlUnary uo)
            {
                //兼容类型，无需转换
                switch (((SqlDataType<NpgsqlDbType>)uo.SqlType).Category)
                {
                    case DBTypeCategory.Text:
                        if (uo.Operand.ClrType == typeof(char) || uo.Operand.ClrType == typeof(string))
                        {
                            Visit(uo.Operand);
                            return uo;
                        }
                        break;
                    case DBTypeCategory.Numeric:
                        //if (uo.Operand.ClrType == typeof(int) || uo.Operand.ClrType == typeof(Int16) ||
                        //   uo.Operand.ClrType == typeof(long) || uo.Operand.ClrType == typeof(byte))
                        if (uo.Operand.ClrType.IsValueType)
                        {
                            Visit(uo.Operand);
                            return uo;
                        }
                        break;
                }


                sb.Append("CAST(");
                QueryFormatOptions none = QueryFormatOptions.None;
                if (uo.Operand.SqlType.CanSuppressSizeForConversionToString)
                {
                    none = QueryFormatOptions.SuppressSize;
                }
                Visit(uo.Operand);
                sb.Append(" AS ");
                sb.Append(uo.SqlType.ToQueryString(none));
                sb.Append(")");
                return uo;
            }

            public override string GetOperator(SqlNodeType nt)
            {
                if (nt == SqlNodeType.LongCount)
                    return "COUNT";
                return base.GetOperator(nt);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {

                if (uo.NodeType == SqlNodeType.Min || uo.NodeType == SqlNodeType.Max ||
                    uo.NodeType == SqlNodeType.Avg || uo.NodeType == SqlNodeType.Sum)
                {
                    Debug.Assert(uo.Operand != null);

                    sb.Append("COALESCE(");
                    sb.Append(GetOperator(uo.NodeType));
                    sb.Append("(");
                    Visit(uo.Operand);

                    //============================================================
                    // 说明：增加对类型的判断
                    var defaultValue = TypeSystem.GetDefaultValue(uo.ClrType);
                    if (defaultValue is DateTime)
                        sb.Append(string.Format("), CAST('{0}' AS DATE))", ((DateTime)defaultValue).ToShortDateString()));
                    else if (defaultValue is string)
                        sb.Append(string.Format("),'{0}')", defaultValue));
                    else
                        sb.Append(string.Format("),{0})", defaultValue));
                    //============================================================
                    //sb.Append("),0)"); 旧代码
                    //============================================================

                    return uo;
                }

                return base.VisitUnaryOperator(uo);
            }

            //将Select.Top等价于limit，将其后置。
            internal override SqlSelect VisitSelect(SqlSelect ss)
            {
                if (!ss.DoNotOutput)
                {
                    string str = null;
                    if (ss.From != null)
                    {
                        StringBuilder oldsb = this.sb;
                        this.sb = new StringBuilder();
                        if (this.IsSimpleCrossJoinList(ss.From))
                        {
                            this.VisitCrossJoinList(ss.From);
                        }
                        else
                        {
                            this.Visit(ss.From);
                        }
                        str = this.sb.ToString();
                        this.sb = oldsb;
                    }
                    this.sb.Append("SELECT ");
                    if (ss.IsDistinct)
                    {
                        this.sb.Append("DISTINCT ");
                    }

                    if (ss.Row.Columns.Count > 0)
                    {
                        this.VisitRow(ss.Row);
                    }
                    else if (this.isDebugMode)
                    {
                        this.Visit(ss.Selection);
                    }
                    else
                    {
                        sb.Append("NULL AS ");
                        sb.Append(SqlIdentifier.QuoteCompoundIdentifier("EMPTY"));
                    }
                    if (str != null)
                    {
                        this.NewLine();
                        this.sb.Append("FROM ");
                        this.sb.Append(str);
                    }
                    if (ss.Where != null)
                    {
                        this.NewLine();
                        this.sb.Append("WHERE ");
                        this.Visit(ss.Where);
                    }
                    if (ss.GroupBy.Count > 0)
                    {
                        this.NewLine();
                        this.sb.Append("GROUP BY ");
                        int num = 0;
                        int count = ss.GroupBy.Count;
                        while (num < count)
                        {
                            SqlExpression node = ss.GroupBy[num];
                            if (num > 0)
                            {
                                this.sb.Append(", ");
                            }
                            this.Visit(node);
                            num++;
                        }
                        if (ss.Having != null)
                        {
                            this.NewLine();
                            this.sb.Append("HAVING ");
                            this.Visit(ss.Having);
                        }
                    }
                    if ((ss.OrderBy.Count > 0) && (ss.OrderingType != SqlOrderingType.Never))
                    {
                        this.NewLine();
                        this.sb.Append("ORDER BY ");
                        int num3 = 0;
                        int num4 = ss.OrderBy.Count;
                        while (num3 < num4)
                        {
                            SqlOrderExpression expression2 = ss.OrderBy[num3];
                            if (num3 > 0)
                            {
                                this.sb.Append(", ");
                            }
                            this.Visit(expression2.Expression);
                            if (expression2.OrderType == SqlOrderType.Descending)
                            {
                                this.sb.Append(" DESC");
                            }
                            num3++;
                        }
                    }
                    //Select.Top后置！
                    if (ss.Top != null)
                    {
                        this.NewLine();
                        //this.sb.Append("LIMIT ");
                        this.Visit(ss.Top);
                        this.sb.Append(" ");
                    }
                }
                return ss;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                if (bo.NodeType == SqlNodeType.Coalesce)
                {
                    this.sb.Append("COALESCE(");
                    this.Visit(bo.Left);
                    this.sb.Append(",");
                    this.Visit(bo.Right);
                    this.sb.Append(")");
                    return bo;
                }

                this.VisitWithParens(bo.Left, bo);
                this.sb.Append(" ");

                //将string + string的运算符转换为“||”
                if ((bo.NodeType == SqlNodeType.Concat || bo.NodeType == SqlNodeType.Add) &&
                    (bo.Left.ClrType == typeof(string) || bo.Left.ClrType == typeof(string)))
                    sb.Append("||");
                else
                    this.sb.Append(this.GetOperator(bo.NodeType));

                this.sb.Append(" ");
                this.VisitWithParens(bo.Right, bo);
                return bo;
            }
        }
    }
}
