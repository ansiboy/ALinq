using System;
using System.Diagnostics;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    class OracleFormatter : DbFormatter
    {
        internal class Visitor : SqlFormatter.Visitor
        {
            public override void FormatValue(object value)
            {
                base.FormatValue(value);
                prevConvert = ConvertType.None;
            }

            internal override SqlSelect VisitSelect(SqlSelect ss)
            {
                if (!ss.DoNotOutput)
                {
                    string str = null;
                    if (ss.From != null)
                    {
                        StringBuilder oldsb = sb;
                        sb = new StringBuilder();
                        if (IsSimpleCrossJoinList(ss.From))
                        {
                            VisitCrossJoinList(ss.From);
                        }
                        else
                        {
                            Visit(ss.From);
                        }
                        str = sb.ToString();
                        sb = oldsb;
                    }
                    else
                    {
                        str = "DUAL";
                    }
                    sb.Append("SELECT ");
                    if (ss.IsDistinct)
                    {
                        sb.Append("DISTINCT ");
                    }

                    if (ss.Row.Columns.Count > 0)
                    {
                        VisitRow(ss.Row);
                    }
                    else if (isDebugMode)
                    {
                        Visit(ss.Selection);
                    }
                    else
                    {
                        sb.Append("NULL AS EMPTY");
                    }
                    if (str != null)
                    {
                        NewLine();
                        sb.Append("FROM ");
                        sb.Append(str);
                    }
                    if (ss.Where != null)
                    {
                        NewLine();
                        sb.Append("WHERE ");
                        Visit(ss.Where);
                    }
                    //Select.Top Convert
                    if (ss.Top != null)
                    {
                        NewLine();
                        if (ss.Where != null)
                            sb.Append("And (");
                        else
                            sb.Append("Where");
                        sb.Append(" ROWNUM <=  ");
                        Visit(ss.Top);
                        if (ss.Where != null)
                            sb.Append(" )");
                    }
                    if (ss.GroupBy.Count > 0)
                    {
                        NewLine();
                        sb.Append("GROUP BY ");
                        int num = 0;
                        int count = ss.GroupBy.Count;
                        while (num < count)
                        {
                            SqlExpression node = ss.GroupBy[num];
                            if (num > 0)
                            {
                                sb.Append(", ");
                            }
                            Visit(node);
                            num++;
                        }
                        if (ss.Having != null)
                        {
                            NewLine();
                            sb.Append("HAVING ");
                            Visit(ss.Having);
                        }
                    }
                    if ((ss.OrderBy.Count > 0) && (ss.OrderingType != SqlOrderingType.Never))
                    {
                        NewLine();
                        sb.Append("ORDER BY ");
                        int num3 = 0;
                        int num4 = ss.OrderBy.Count;
                        while (num3 < num4)
                        {
                            SqlOrderExpression expression2 = ss.OrderBy[num3];
                            if (num3 > 0)
                            {
                                sb.Append(", ");
                            }
                            Visit(expression2.Expression);
                            if (expression2.OrderType == SqlOrderType.Descending)
                            {
                                sb.Append(" DESC");
                            }
                            num3++;
                        }
                    }
                }
                return ss;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                #region MyRegion
                //if (bo.NodeType == SqlNodeType.Coalesce)
                //{
                //    this.sb.Append("COALESCE(");
                //    this.Visit(bo.Left);
                //    this.sb.Append(",");
                //    this.Visit(bo.Right);
                //    this.sb.Append(")");
                //    return bo;
                //} 
                #endregion

                string binaryMethod = string.Empty;
                switch (bo.NodeType)
                {
                    case SqlNodeType.Coalesce:
                        binaryMethod = "COALESCE";
                        break;
                    case SqlNodeType.BitAnd:
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

                VisitWithParens(bo.Left, bo);
                sb.Append(" ");

                //string connector use ||
                if ((bo.NodeType == SqlNodeType.Concat || bo.NodeType == SqlNodeType.Add) &&
                    (bo.Left.ClrType == typeof(string) || bo.Left.ClrType == typeof(string)))
                    sb.Append("||");
                else
                    sb.Append(GetOperator(bo.NodeType));

                sb.Append(" ");
                VisitWithParens(bo.Right, bo);
                return bo;
            }

            protected override void WriteName(string name)
            {
                if (SqlIdentifier.NeedToQuote(name))
                    sb.Append(SqlIdentifier.QuoteCompoundIdentifier(name));
                else
                    sb.Append(name);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                if (uo.NodeType == SqlNodeType.BitNot)
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

                    var defaultValue = TypeSystem.GetDefaultValue(uo.ClrType);
                    if (defaultValue is DateTime)
                        sb.Append(string.Format("), TO_DATE('{0}', 'YYYY-MM-DD'))", ((DateTime)defaultValue).ToString("yyyy-MM-dd")));
                    else if (defaultValue is string)
                        sb.Append(string.Format("),'{0}')", defaultValue));
                    else
                        sb.Append(string.Format("),{0})", defaultValue));

                    //sb.Append("),0)");

                    return uo;
                }
                //if (uo.Operand.NodeType == SqlNodeType.Convert)
                //{
                //    var dbType1 = uo.SqlType;
                //    var dbType2 = uo.SqlType;
                //    if (dbType1.IsNumeric && dbType2.IsNumeric)
                //    {
                //        base.Visit(uo.Operand);
                //        return uo;
                //    }

                //}
                return base.VisitUnaryOperator(uo);
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                if (Mode == SqlProvider.ProviderMode.Oracle)
                {
                    sb.Append(spc.Function.MappedName);
                }
                else
                {
                    sb.Append("begin ");
                    var returnType = spc.Function.Method.ReturnType;
                    if (returnType != typeof(void))
                        sb.Append(":RETURN_VALUE := ");

                    WriteName(spc.Function.MappedName);
                    sb.Append("(");
                    int count = spc.Function.Parameters.Count;
                    for (int i = 0; i < count; i++)
                    {
                        MetaParameter parameter = spc.Function.Parameters[i];
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }
                        Visit(spc.Arguments[i]);
                    }
                    sb.Append("); ");
                    sb.Append("end;");
                }
                return spc;
            }

            enum ConvertType
            {
                None,
                Number,
                Char,
                Date,
            }

     

            private ConvertType prevConvert = ConvertType.None;

            protected override SqlExpression TranslateConverter(SqlUnary uo)
            {
                if (uo.SqlType.IsNumeric)
                {
                    if (prevConvert != ConvertType.Number)
                    {
                        sb.Append("TO_NUMBER(");
                        prevConvert = ConvertType.Number;
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else
                    {
                        Visit(uo.Operand);
                    }
                }
                else if (uo.SqlType.IsString || uo.SqlType.IsChar)
                {
                    if (prevConvert != ConvertType.Char)
                    {
                        prevConvert = ConvertType.Char;
                        sb.Append("TO_CHAR(");
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else
                    {
                        Visit(uo.Operand);
                    }

                }
                else if (uo.ClrType == typeof(DateTime))
                {
                    if (prevConvert != ConvertType.Date)
                    {
                        prevConvert = ConvertType.Date;

                        sb.Append("TO_DATE(");
                        Visit(uo.Operand);
                        sb.Append(",'YYYY-MM-DD HH24:MI:SS')");
                    }
                    else
                    {
                        Visit(uo.Operand);
                    }

                }
                else
                {
                    Visit(uo.Operand);
                }
                return uo;
            }
        }

        public OracleFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }
    }

}
