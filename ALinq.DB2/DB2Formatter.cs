using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ALinq.SqlClient;
using IBM.Data.DB2;

namespace ALinq.DB2
{
    class DB2Formatter : DbFormatter
    {
        public DB2Formatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }

        internal class Visitor : SqlFormatter.Visitor
        {
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
                    if (ss.Top != null)
                    {
                        this.NewLine();
                        this.sb.Append("FETCH FIRST ");
                        this.Visit(ss.Top);
                        this.sb.Append(" ROWS ONLY");
                    }
                }
                return ss;
            }

    
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                if(uo.NodeType == SqlNodeType.BitNot)
                {
                    sb.Append("BITNOT(");
                    Visit(uo.Operand);
                    sb.Append(")");
                    return uo;
                }

                if (uo.NodeType == SqlNodeType.Min || uo.NodeType == SqlNodeType.Max ||
                    uo.NodeType == SqlNodeType.Avg || uo.NodeType == SqlNodeType.Sum)
                {
                    Debug.Assert(uo.Operand != null);

                    sb.Append("NVL(");
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

            protected override SqlExpression TranslateConverter(SqlUnary uo)
            {
                //兼容类型，无需转换
                switch (((SqlDataType<DB2Type>)uo.SqlType).Category)
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

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                string str = null;
                switch (fc.Name)
                {
                    case "AddDays":
                    case "AddHours":
                    case "AddMinutes":
                    case "AddMonths":
                    case "AddSeconds":
                    case "AddYears":
                        str = fc.Name.Substring(3).ToUpper();
                        break;
                    case "AddTimeSpan":
                        Visit(fc.Arguments[0]);
                        sb.Append(" + ");
                        Visit(fc.Arguments[1]);
                        sb.Append(" HOURS");
                        sb.Append(" + ");
                        Visit(fc.Arguments[2]);
                        sb.Append(" MINUTES");
                        sb.Append(" + ");
                        Visit(fc.Arguments[3]);
                        sb.Append(" SECONDS");
                        return fc;
                }
                if (str != null)
                {
                    Visit(fc.Arguments[0]);
                    sb.Append(" + ");
                    Visit(fc.Arguments[1]);
                    sb.Append(" ");
                    sb.Append(str);
                    return fc;
                }

                return base.VisitFunctionCall(fc);
            }


            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                string binaryMethod = string.Empty;
                switch (bo.NodeType)
                {
                    case SqlNodeType.Coalesce:
                        #region MyRegion
                        //this.sb.Append("COALESCE(");
                        //this.Visit(bo.Left);
                        //this.sb.Append(",");
                        //this.Visit(bo.Right);
                        //this.sb.Append(")");
                        //return bo; 
                        #endregion
                        binaryMethod = "COALESCE";
                        break;
                    case SqlNodeType.BitAnd:
                        #region MyRegion
                        //this.sb.Append("BITAND(");
                        //this.Visit(bo.Left);
                        //this.sb.Append(",");
                        //this.Visit(bo.Right);
                        //this.sb.Append(")");
                        //return bo; 
                        #endregion
                        binaryMethod = "BITAND";
                        break;
                    case SqlNodeType.BitOr:
                        binaryMethod = "BITOR";
                        break;
                    case SqlNodeType.BitXor:
                        binaryMethod = "BITXOR";
                        break;
                }

                if (binaryMethod != string.Empty)
                {
                    this.sb.Append(string.Format("{0}(", binaryMethod));
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
