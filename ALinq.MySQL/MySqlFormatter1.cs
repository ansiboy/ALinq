using System.Diagnostics;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;
using MySql.Data.MySqlClient;

namespace ALinq.MySQL
{
    class MySqlFormatter : DbFormatter
    {
        private readonly Visitor visitor;

        public MySqlFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }

        private class Visitor : SqlFormatter.Visitor
        {
            //
            protected override void WriteName(string name)
            {
                sb.Append(MySqlIdentifier.QuoteCompoundIdentifier(name));
            }

            //internal override string GetBoolValue(bool value)
            //{
            //    if (value)
            //        return "true";
            //    return "false";
            //}

            internal override SqlExpression VisitValue(SqlValue sqlValue)
            {
                if (sqlValue.Value == null)
                {
                    object value = null;
                    if (sqlValue.SqlType.IsNumeric)
                        value = 0;
                    sqlValue = new SqlValue(sqlValue.ClrType, sqlValue.SqlType, value,
                                            sqlValue.IsClientSpecified, sqlValue.SourceExpression);
                }
                return base.VisitValue(sqlValue);
            }

            //每写完一句，用分号分隔。
            internal override SqlBlock VisitBlock(SqlBlock block)
            {
                int num = 0;
                int count = block.Statements.Count;
                while (num < count)
                {
                    this.Visit(block.Statements[num]);
                    if (num < (count - 1))
                    {
                        var select = block.Statements[num + 1] as SqlSelect;
                        if ((select == null) || !select.DoNotOutput)
                        {
                            sb.Append(";");
                            this.NewLine();
                            this.NewLine();
                        }
                    }
                    num++;
                }
                return block;
            }

            //取消函数括号，即Limit(int,int) -> Limit int, int
            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                switch (fc.Name)
                {
                    case "Limit":
                        {

                            Debug.Assert(fc.Arguments.Count == 2);
                            bool limited = false;
                            if (!(fc.Arguments[0] is SqlValue) || (int)((SqlValue)fc.Arguments[0]).Value > 0)
                            {
                                sb.Append("LIMIT ");
                                this.Visit(fc.Arguments[0]);
                                sb.Append(" ");
                                limited = true;
                            }

                            if (!(fc.Arguments[1] is SqlValue) || (int)((SqlValue)fc.Arguments[1]).Value > 0)
                            {
                                if (!limited)
                                    sb.Append("LIMIT 18446744073709551615 ");

                                sb.Append("OFFSET ");
                                this.Visit(fc.Arguments[1]);
                            }

                            return fc;
                        }
                    //case "DATE_FORMAT":
                    //    sb.Append("CAST(");
                    //    var result = base.VisitFunctionCall(fc);
                    //    sb.Append(" AS SIGNED)");
                    //    return result;
                    //case "Date":
                    //    sb.Append("Date(");
                    //    Visit(fc.Arguments[0]);
                    //    sb.Append(")");
                    //    return fc;
                    case "AddDays":
                    case "AddHours":
                    case "AddMinutes":
                    case "AddMonths":
                    case "AddSeconds":
                    case "AddYears":
                        sb.Append("DATE_ADD(");
                        Visit(fc.Arguments[0]);
                        sb.Append(", INTERVAL ");
                        Visit(fc.Arguments[1]);
                        sb.Append(" ");
                        sb.Append(((SqlValue)fc.Arguments[2]).Value);
                        sb.Append(")");
                        return fc;
                    //case "AddHours":
                    //    sb.Append("DATE_ADD(");
                    //    Visit(fc.Arguments[0]);
                    //    sb.Append(", INTERVAL ");
                    //    Visit(fc.Arguments[1]);
                    //    sb.Append(" DAY)");
                    //    return fc;
                }
                return base.VisitFunctionCall(fc);
            }

            #region 禁用别名
            //禁用别名，在SQLite里，Update、Delete语句不允许使用别名。
            //例如：DELETE FROM [Categories] AS [t0] WHERE ([t0].[CategoryID] = @p0) AND ([t0].[CategoryName] = @p1)
            private bool disableAlias;          //禁用别名开关。
            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                disableAlias = true;
                var result = base.VisitDelete(sd);
                disableAlias = false;
                return result;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate su)
            {
                disableAlias = true;
                var result = base.VisitUpdate(su);
                disableAlias = false;
                return result;
            }

            internal override SqlAlias VisitAlias(SqlAlias alias)
            {
                bool flag = alias.Node is SqlSelect;
                int sourceDepth = this.depth;
                string str;
                string name = "";
                SqlTable node = alias.Node as SqlTable;
                if (node != null)
                {
                    name = node.Name;
                }
                if (alias.Name == null)
                {
                    if (!this.names.TryGetValue(alias, out str))
                    {
                        str = "A" + this.names.Count;
                        this.names[alias] = str;
                    }
                }
                else
                {
                    str = alias.Name;
                }
                if (flag)
                {
                    this.depth++;
                    this.sb.Append("(");
                    this.NewLine();
                }
                this.Visit(alias.Node);
                if (flag)
                {
                    this.NewLine();
                    this.sb.Append(")");
                    this.depth = sourceDepth;
                }
                if ((!this.suppressedAliases.Contains(alias) && (str != null)) && (name != str) && !disableAlias)
                {
                    this.sb.Append(" AS ");
                    this.WriteName(str);
                }
                return alias;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                string str = null;
                SqlColumn key = cref.Column;
                SqlAlias alias = key.Alias;
                if (alias == null)
                {
                    this.aliasMap.TryGetValue(key, out alias);
                }
                if (alias != null && !disableAlias)
                {
                    if (alias.Name == null)
                    {
                        if (!this.names.TryGetValue(alias, out str))
                        {
                            str = "A" + this.names.Count;
                            this.names[key.Alias] = str;
                        }
                    }
                    else
                    {
                        str = key.Alias.Name;
                    }
                }
                if (!this.suppressedAliases.Contains(key.Alias) && (!string.IsNullOrEmpty(str)))
                {
                    this.WriteName(str);
                    this.sb.Append(".");
                }
                string name = key.Name;
                string str3 = this.InferName(key.Expression, null);
                if (name == null)
                {
                    name = str3;
                }
                if ((name == null) && !this.names.TryGetValue(key, out name))
                {
                    name = "C" + this.names.Count;
                    this.names[key] = name;
                }
                this.WriteName(name);
                return cref;
            }
            #endregion

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                //if (uo.NodeType == SqlNodeType.Convert)
                //{
                //   
                //    base.Visit(uo.Operand);
                //    return uo;
                //}
                if (uo.NodeType == SqlNodeType.Min || uo.NodeType == SqlNodeType.Max ||
                    uo.NodeType == SqlNodeType.Avg || uo.NodeType == SqlNodeType.Sum)
                {
                    //if (!TypeSystem.IsNullableType(uo.ClrType))
                    //{
                    sb.Append("IFNULL(");
                    sb.Append(GetOperator(uo.NodeType));
                    sb.Append("(");
                    if (uo.Operand == null)
                        sb.Append("*");
                    else
                        Visit(uo.Operand);
                    sb.Append("),0)");
                    return uo;
                }
                return base.VisitUnaryOperator(uo);
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
                var sqlType = (MySqlDataType)uo.SqlType;
                switch (sqlType.Category)
                {
                    case DBTypeCategory.Binary:
                        sb.Append("BINARY");
                        break;
                    case DBTypeCategory.Numeric:
                        switch (sqlType.SqlDbType)
                        {
                            case MySqlDbType.Decimal:
                            case MySqlDbType.Double:
                            case MySqlDbType.Float:
                                sb.Append("DECIMAL");
                                break;
                            default:
                                if (sqlType.SqlDbType.ToString()[0] == 'U')
                                    sb.Append("UNSIGNED");
                                else
                                    sb.Append("SIGNED");
                                break;
                        }
                        break;
                    case DBTypeCategory.Text:
                        sb.Append("CHAR");
                        break;
                    case DBTypeCategory.DateTime:
                        sb.Append("DATETIME");
                        break;
                }
                sb.Append(")");
                return uo;
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
            //            this.sb.Append(", ");
            //        }
            //        if (key is SqlDynamicColumn)
            //        {
            //            Debug.Assert(key.Expression == null);
            //            this.WriteName(name);
            //        }
            //        else
            //        {
                       
            //        }
            //        num++;
            //    }
            //    return row;
            //}

            internal override void VisitRowColumn(SqlColumn key)
            {
                this.Visit(key.Expression);
                string name = key.Name;
                string str2 = this.InferName(key.Expression, null);
                if (name == null)
                {
                    name = str2;
                }
                if ((name == null) && !this.names.TryGetValue(key, out name))
                {
                    name = "C" + this.names.Count;
                    this.names[key] = name;
                }
                if ((name != str2) && !string.IsNullOrEmpty(name))
                {
                    if (string.IsNullOrEmpty(str2) && key.Expression == null)
                    {
                        this.WriteName(name);
                    }
                    else
                    {
                        this.sb.Append(" AS ");
                        this.WriteName(name);
                    }
                }
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
                        //MySQL不允许[EMPTY]，改为EMPTY
                        this.sb.Append("NULL AS EMPTY");
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
                        this.Visit(ss.Top);
                        this.sb.Append(" ");
                    }
                }
                return ss;
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                //this.WriteName(spc.Function.MappedName);
                sb.Append(spc.Function.MappedName);
                return spc;
            }


        }
    }
}