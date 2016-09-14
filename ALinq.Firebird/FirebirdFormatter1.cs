using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.Firebird
{
    class FirebirdFormatter : DbFormatter
    {
        public FirebirdFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }

        class Visitor : SqlFormatter.Visitor
        {
            internal override SqlSelect VisitSelect(SqlSelect ss)
            {
                if (!ss.DoNotOutput)
                {
                    string str = null;
                    if (ss.From != null)
                    {
                        StringBuilder tmp = this.sb;
                        this.sb = new StringBuilder();
                        if (IsSimpleCrossJoinList(ss.From))
                        {
                            VisitCrossJoinList(ss.From);
                        }
                        else
                        {
                            Visit(ss.From);
                        }
                        str = this.sb.ToString();
                        this.sb = tmp;
                    }
                    else
                    {
                        str = "RDB$DATABASE";
                    }
                    this.sb.Append("SELECT ");
                    if (ss.IsDistinct)
                    {
                        sb.Append("DISTINCT ");
                    }
                    if (ss.Top != null)
                    {
                        if (ss.Top is SqlFunctionCall == false)
                            sb.Append("FIRST ");
                        Visit(ss.Top);
                        sb.Append(" ");
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

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                switch (fc.Name)
                {
                    case "SkipTake":
                        {
                            Debug.Assert(fc.Arguments.Count == 2);
                            var skip = (int)((SqlValue)fc.Arguments[0]).Value;
                            var take = (int)((SqlValue)fc.Arguments[1]).Value;
                            var str = string.Empty;
                            if (take >= 0)
                            {
                                sb.Append("FIRST ");
                                sb.Append(take);
                                str = " ";
                            }
                            if (skip >= 0)
                            {
                                sb.Append(str);
                                sb.Append("SKIP ");
                                sb.Append(skip);
                            }
                            return fc;
                        }

                    case "LEN":
                    case "DATALENGTH":
                        sb.Append("OCTET_LENGTH( ");
                        Visit(fc.Arguments[0]);
                        sb.Append(" ) ");
                        return fc;
                    case "TrimEnd":
                        sb.Append("TRIM( TRAILING From ");
                        Visit(fc.Arguments[0]);
                        sb.Append(" )");
                        return fc;
                    case "TrimStart":
                        sb.Append("TRIM( LEADING From ");
                        Visit(fc.Arguments[0]);
                        sb.Append(" )");
                        return fc;
                    case "SUBSTRING":
                        sb.Append("SUBSTRING( ");
                        Visit(fc.Arguments[0]);
                        sb.Append(" FROM ");
                        Visit(fc.Arguments[1]);
                        if (fc.Arguments.Count > 2)
                        {
                            sb.Append(" FOR ");
                            Visit(fc.Arguments[2]);
                        }
                        sb.Append(")");
                        return fc;
                    case "Date":
                        sb.Append("CAST (");
                        Visit(fc.Arguments[0]);
                        sb.Append(" AS DATE)");
                        return fc;
                }
                //if (fc.Name.StartsWith("GEN_ID"))
                //{
                //    sb.Append(fc.Name);
                //    sb.Append(" FROM RDB$DATABASE");
                //    return fc;
                //}
                return base.VisitFunctionCall(fc);
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
                VisitWithParens(bo.Left, bo);
                this.sb.Append(" ");

                if ((bo.NodeType == SqlNodeType.Concat || bo.NodeType == SqlNodeType.Add) &&
                    (bo.Left.ClrType == typeof(string) || bo.Left.ClrType == typeof(string)))
                    sb.Append("||");
                else
                    this.sb.Append(GetOperator(bo.NodeType));

                this.sb.Append(" ");
                this.VisitWithParens(bo.Right, bo);
                return bo;
            }

            //internal override SqlRow VisitRow(SqlRow row)
            //{
            //    int num = 0;
            //    int count = row.Columns.Count;
            //    while (num < count)
            //    {
            //        SqlColumn key = row.Columns[num];
            //        if (num > 0)
            //        {
            //            sb.Append(", ");
            //        }

            //        num++;
            //    }
            //    return row;
            //}

            internal override void VisitRowColumn(SqlColumn key)
            {
                Visit(key.Expression);
                string name = key.Name;
                string str2 = InferName(key.Expression, null);
                if (name == null)
                {
                    name = str2;
                }
                if ((name == null) && !names.TryGetValue(key, out name))
                {
                    name = "C" + names.Count;
                    names[key] = name;
                }

                if (key.Expression != null && key.Expression.NodeType == SqlNodeType.FunctionCall &&
                    ((SqlFunctionCall)key.Expression).Name.StartsWith("GEN_ID"))
                    name = null;

                if (!string.IsNullOrEmpty(str2))
                    str2 = SqlIdentifier.UnquoteIdentifier(str2);

                if (name != str2 && !string.IsNullOrEmpty(name))
                {
                    if (string.IsNullOrEmpty(str2) && key.Expression == null)
                    {
                        this.WriteName(name);
                    }
                    else
                    {
                        sb.Append(" AS ");

                        if (SqlIdentifier.NeedToQuote(name))
                            WriteName(SqlIdentifier.QuoteIdentifier(name));
                        else
                            WriteName(name);
                    }
                }
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                //if (uo.NodeType == SqlNodeType.Convert)
                //{
                //    this.Visit(uo.Operand);
                //    return uo;
                //}
                if (uo.NodeType == SqlNodeType.Min || uo.NodeType == SqlNodeType.Max ||
                   uo.NodeType == SqlNodeType.Avg || uo.NodeType == SqlNodeType.Sum)
                {
                    //IFNULL
                    //if (!TypeSystem.IsNullableType(uo.ClrType))
                    //{
                    sb.Append("COALESCE(");
                    sb.Append(GetOperator(uo.NodeType));
                    sb.Append("(");
                    if (uo.Operand == null)
                        sb.Append("*");
                    else
                        Visit(uo.Operand);
                    sb.Append("),0)");
                    return uo;
                    //}
                }
                return base.VisitUnaryOperator(uo);
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                WriteName(spc.Function.MappedName);
                return spc;
            }

            protected override void WriteName(string name)
            {
                var index = name.LastIndexOf('.');
                var columnName = index > 0 ? name.Substring(index + 1) : name;

                if (SqlIdentifier.NeedToQuote(columnName))
                    sb.Append(SqlIdentifier.QuoteCompoundIdentifier(columnName));
                else
                    sb.Append(name);
            }

            public override void FormatValue(object value)
            {
                if (value is Enum)
                {
                    sb.Append(((int)value));
                    return;
                }
                base.FormatValue(value);
            }

            protected override SqlExpression TranslateConverter(SqlUnary uo)
            {
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

        }


    }
}