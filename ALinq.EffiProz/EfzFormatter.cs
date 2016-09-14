using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.EffiProz
{
    class EfzFormatter : DbFormatter
    {
        public EfzFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }

        class Visitor : SqlFormatter.Visitor
        {
            //internal override string GetBoolValue(bool value)
            //{
            //    return value ? "true" : "false";
            //}

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                if (uo.NodeType == SqlNodeType.Min || uo.NodeType == SqlNodeType.Max ||
                    uo.NodeType == SqlNodeType.Avg || uo.NodeType == SqlNodeType.Sum)
                {
                    //if (!TypeSystem.IsNullableType(uo.ClrType))
                    //{
                    Debug.Assert(uo.Operand != null);

                    sb.Append("IFNULL(");
                    sb.Append(GetOperator(uo.NodeType));
                    sb.Append("(");
                    Visit(uo.Operand);
                    sb.Append("),0)");
                    return uo;
                    //}
                }
                return base.VisitUnaryOperator(uo);
            }

            protected override SqlExpression TranslateConverter(SqlUnary uo)
            {
                sb.Append("CONVERT(");
                QueryFormatOptions none = QueryFormatOptions.None;
                if (uo.Operand.SqlType.CanSuppressSizeForConversionToString)
                {
                    none = QueryFormatOptions.SuppressSize;
                }
                Visit(uo.Operand);
                sb.Append(",");
                sb.Append(uo.SqlType.ToQueryString(none));
                sb.Append(")");
                return uo;
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                switch (fc.Name)
                {
                    case "SUBSTRING":
                        sb.Append(fc.Name);
                        sb.Append("(");
                        Visit(fc.Arguments[0]);
                        sb.Append(" FROM ");
                        Visit(fc.Arguments[1]);
                        if (fc.Arguments.Count == 3)
                        {
                            sb.Append(" FOR ");
                            Visit(fc.Arguments[2]);
                        }
                        sb.Append(")");
                        return fc;

                    case "POSITION":
                        sb.Append(fc.Name);
                        sb.Append("(");
                        Visit(fc.Arguments[0]);
                        sb.Append(" IN ");
                        Visit(fc.Arguments[1]);
                        sb.Append(")");
                        return fc;

                    //取消函数括号，即Limit(int,int) -> Limit int, int
                    case "Limit":
                        {
                            Debug.Assert(fc.Arguments.Count == 2);
                            sb.Append(fc.Name + " ");

                            this.Visit(fc.Arguments[0]);
                            if (fc.Arguments[1] != null)
                            {
                                //this.sb.Append(" OFFSET ");
                                sb.Append(" ");
                                Visit(fc.Arguments[1]);
                            }

                            return fc;
                        }
                }
                return base.VisitFunctionCall(fc);
            }

            internal override SqlSelect VisitSelect(SqlSelect ss)
            {
                if (!ss.DoNotOutput)
                {
                    string str = null;
                    if (ss.From != null)
                    {
                        StringBuilder sb = this.sb;
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
                        this.sb = sb;
                    }
                    this.sb.Append("SELECT ");
                    if (ss.IsDistinct)
                    {
                        this.sb.Append("DISTINCT ");
                    }
                    if (ss.Top != null)
                    {
                        this.Visit(ss.Top);
                        this.sb.Append(" ");
                        if (ss.IsPercent)
                        {
                            this.sb.Append(" PERCENT ");
                        }
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
                        //this.sb.Append("NULL AS [EMPTY]");
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
                }
                return ss;
            }




        }


    }
}
