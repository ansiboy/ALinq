using System;
using System.Diagnostics;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.SQLite
{
    class SQLiteFormatter : DbFormatter
    {
        // Fields
        private class Visitor : SqlFormatter.Visitor
        {
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
                        this.sb.Append("NULL AS [EMPTY]");
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

            //取消函数括号，即Limit(int,int) -> Limit int, int
            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                switch (fc.Name)
                {
                    case "Limit":
                        {
                            sb.Append(fc.Name + " ");

                            int num = 0;
                            int count = fc.Arguments.Count;
                            while (num < count)
                            {
                                if (num > 0)
                                    this.sb.Append(", ");

                                this.Visit(fc.Arguments[num]);
                                num++;
                            }
                            return fc;
                        }
                    //case "AddYears":
                    //    {
                    //        sb.Append("Date(");
                    //        Visit(fc.Arguments[0]);
                    //        sb.Append(", '+");
                    //        //Visit(fc.Arguments[1]);
                    //        sb.Append("10 years')");
                    //    }
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
                var node = alias.Node as SqlTable;
                if (node != null)
                {
                    name = node.Name;
                }
                if (alias.Name == null)
                {
                    if (!names.TryGetValue(alias, out str))
                    {
                        str = "A" + this.names.Count;
                        names[alias] = str;
                    }
                }
                else
                {
                    str = alias.Name;
                }
                if (flag)
                {
                    depth++;
                    sb.Append("(");
                    NewLine();
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
                if (!this.suppressedAliases.Contains(key.Alias) && !string.IsNullOrEmpty(str))
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

            //禁用转换
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                //dont's use convert function
                if (uo.NodeType == SqlNodeType.Convert)
                {
                    Visit(uo.Operand);
                    return uo;
                }
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

                    //============================================================
                    // 说明：增加对类型的判断
                    var defaultValue = TypeSystem.GetDefaultValue(uo.ClrType);
                    if (defaultValue is DateTime)
                        sb.Append(string.Format("), DATE('{0}'))", ((DateTime)defaultValue).ToShortDateString()));
                    else if (defaultValue is string)
                        sb.Append(string.Format("),'{0}')", defaultValue));
                    else
                        sb.Append(string.Format("),{0})", defaultValue));
                    //============================================================
                    //sb.Append("),0)"); 旧代码
                    //============================================================
   
                    return uo;
                    //}
                }
                return base.VisitUnaryOperator(uo);
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                return rowNumber;
            }
        }

        public SQLiteFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }
    }

}
