using System;
using System.Collections.Generic;
using System.Diagnostics;
using ALinq.Mapping;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ALinq.SqlClient
{
    internal abstract class SqlFormatter : DbFormatter
    {
        protected SqlFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override Visitor CreateVisitor()
        {
            return new Visitor();
        }


        // Nested Types
        private class AliasMapper : SqlVisitor
        {
            // Fields
            private Dictionary<SqlColumn, SqlAlias> aliasMap;
            private SqlAlias currentAlias;

            // Methods
            internal AliasMapper(Dictionary<SqlColumn, SqlAlias> aliasMap)
            {
                this.aliasMap = aliasMap;
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                SqlAlias currentAlias = this.currentAlias;
                this.currentAlias = a;
                base.VisitAlias(a);
                this.currentAlias = currentAlias;
                return a;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                this.aliasMap[col] = this.currentAlias;
                this.Visit(col.Expression);
                return col;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                foreach (SqlColumn column in row.Columns)
                {
                    this.VisitColumn(column);
                }
                return row;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                foreach (SqlColumn column in tab.Columns)
                {
                    this.VisitColumn(column);
                }
                return tab;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                foreach (SqlColumn column in fc.Columns)
                {
                    this.VisitColumn(column);
                }
                return fc;
            }


        }

        internal class Visitor : SqlVisitor
        {
            // Fields
            protected internal Dictionary<SqlColumn, SqlAlias> aliasMap = new Dictionary<SqlColumn, SqlAlias>();
            protected internal int depth;
            protected internal bool isDebugMode;
            protected internal Dictionary<SqlNode, string> names = new Dictionary<SqlNode, string>();
            protected internal bool parenthesizeTop;
            protected internal StringBuilder sb;
            protected internal List<SqlSource> suppressedAliases = new List<SqlSource>();
            private SqlIdentifier sqlIdentifier;


            // Methods
            public Visitor()
            {
                //this.sqlIdentity = sqlIdentity;
            }


            internal SqlIdentifier SqlIdentifier
            {
                get
                {
                    if (sqlIdentifier == null)
                        sqlIdentifier = new ALinq.SqlClient.MsSqlIdentifier();
                    return sqlIdentifier;
                }
                set
                {
                    sqlIdentifier = value;
                }
            }

            internal virtual string EscapeSingleQuotes(string str)
            {
                if (str.IndexOf('\'') < 0)
                {
                    return str;
                }
                var builder = new StringBuilder();
                foreach (char ch in str)
                {
                    if (ch == '\'')
                    {
                        builder.Append("''");
                    }
                    else
                    {
                        builder.Append("'");
                    }
                }
                return builder.ToString();
            }

            public string Format(SqlNode node)
            {
                return this.Format(node, false);
            }

            public string Format(SqlNode node, bool isDebug)
            {
                this.sb = new StringBuilder();
                this.isDebugMode = isDebug;
                this.aliasMap.Clear();
                if (isDebug)
                {
                    new SqlFormatter.AliasMapper(this.aliasMap).Visit(node);
                }
                this.Visit(node);
                return this.sb.ToString();
            }

            private void FormatType(IProviderType type)
            {
                this.sb.Append(type.ToQueryString());
            }

            public virtual void FormatValue(object value)
            {
                if (value == null)
                {
                    this.sb.Append("NULL");
                }
                else
                {
                    Type type = value.GetType();
                    if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        type = type.GetGenericArguments()[0];
                    }
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Object:
                            if (!(value is Guid))
                            {
                                Type type2 = value as Type;
                                if (type2 != null)
                                {
                                    if (this.isDebugMode)
                                    {
                                        this.sb.Append("typeof(");
                                        this.sb.Append(type2.Name);
                                        this.sb.Append(")");
                                        return;
                                    }
                                    this.FormatValue("");
                                    return;
                                }
                                break;
                            }
                            this.sb.Append("'");
                            this.sb.Append(value);
                            this.sb.Append("'");
                            return;

                        case TypeCode.Boolean:
                            this.sb.Append(this.GetBoolValue((bool)value));
                            return;

                        case TypeCode.Char:
                        case TypeCode.DateTime:
                        case TypeCode.String:
                            this.sb.Append("'");
                            this.sb.Append(this.EscapeSingleQuotes(value.ToString()));
                            this.sb.Append("'");
                            return;

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            this.sb.Append(value);
                            return;
                    }
                    if (!this.isDebugMode)
                    {
                        throw Error.ValueHasNoLiteralInSql(value);
                    }
                    this.sb.Append("value(");
                    this.sb.Append(value.ToString());
                    this.sb.Append(")");
                }
            }

            internal virtual string GetBoolValue(bool value)
            {
                if (!value)
                {
                    return "0";
                }
                return "1";
            }

            public virtual string GetOperator(SqlNodeType nt)
            {
                switch (nt)
                {
                    case SqlNodeType.Add:
                        return "+";

                    case SqlNodeType.And:
                        return "AND";

                    case SqlNodeType.Avg:
                        return "AVG";

                    case SqlNodeType.BitAnd:
                        return "&";

                    case SqlNodeType.BitNot:
                        return "~";

                    case SqlNodeType.BitOr:
                        return "|";

                    case SqlNodeType.BitXor:
                        return "^";

                    case SqlNodeType.ClrLength:
                        return "CLRLENGTH";

                    case SqlNodeType.Concat:
                        return "+";

                    case SqlNodeType.Count:
                        return "COUNT";

                    case SqlNodeType.Covar:
                        return "COVAR";

                    case SqlNodeType.EQ:
                        return "=";

                    case SqlNodeType.EQ2V:
                        return "=";

                    case SqlNodeType.IsNotNull:
                        return "IS NOT NULL";

                    case SqlNodeType.IsNull:
                        return "IS NULL";

                    case SqlNodeType.LE:
                        return "<=";

                    case SqlNodeType.LongCount:
                        switch (this.Mode)
                        {
                            case SqlProvider.ProviderMode.Sql2000:
                            case SqlProvider.ProviderMode.Sql2005:
                            case SqlProvider.ProviderMode.SqlCE:
                                return "COUNT_BIG";
                            default:
                                return "COUNT";
                        }
                    case SqlNodeType.LT:
                        return "<";

                    case SqlNodeType.GE:
                        return ">=";

                    case SqlNodeType.GT:
                        return ">";

                    case SqlNodeType.Max:
                        return "MAX";

                    case SqlNodeType.Min:
                        return "MIN";

                    case SqlNodeType.Mod:
                        return "%";

                    case SqlNodeType.Mul:
                        return "*";

                    case SqlNodeType.NE:
                        return "<>";

                    case SqlNodeType.NE2V:
                        return "<>";

                    case SqlNodeType.Negate:
                        return "-";

                    case SqlNodeType.Not:
                        return "NOT";

                    case SqlNodeType.Not2V:
                        return "NOT";

                    case SqlNodeType.Or:
                        return "OR";

                    case SqlNodeType.Div:
                        return "/";

                    case SqlNodeType.Stddev:
                        return "STDEV";

                    case SqlNodeType.Sub:
                        return "-";

                    case SqlNodeType.Sum:
                        return "SUM";
                }
                throw Error.InvalidFormatNode(nt);
            }

            protected virtual string InferName(SqlExpression exp, string def)
            {
                if (exp == null)
                {
                    return null;
                }
                SqlNodeType nodeType = exp.NodeType;
                switch (nodeType)
                {
                    case SqlNodeType.Column:
                        return ((SqlColumn)exp).Name;

                    case SqlNodeType.ColumnRef:
                        return ((SqlColumnRef)exp).Column.Name;

                    case SqlNodeType.ExprSet:
                        return this.InferName(((SqlExprSet)exp).Expressions[0], def);
                }
                if (nodeType != SqlNodeType.Member)
                {
                    return def;
                }
                return ((SqlMember)exp).Member.Name;
            }

            protected internal virtual bool IsSimpleCrossJoinList(SqlNode node)
            {
                var join = node as SqlJoin;
                if (join != null)
                {
                    return (((join.JoinType == SqlJoinType.Cross) && IsSimpleCrossJoinList(join.Left)) &&
                              IsSimpleCrossJoinList(join.Right));
                }
                var alias = node as SqlAlias;
                return ((alias != null) && (alias.Node is SqlTable));
            }

            protected internal virtual void NewLine()
            {
                if (this.sb.Length > 0)
                {
                    this.sb.AppendLine();
                }
                for (int i = 0; i < this.depth; i++)
                {
                    this.sb.Append("    ");
                }
            }

            internal static bool RequiresOnCondition(SqlJoinType joinType)
            {
                switch (joinType)
                {
                    case SqlJoinType.Cross:
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        return false;

                    case SqlJoinType.Inner:
                    case SqlJoinType.LeftOuter:
                        return true;
                }
                throw Error.InvalidFormatNode(joinType);
            }

            internal override SqlAlias VisitAlias(SqlAlias alias)
            {
                bool flag = alias.Node is SqlSelect;
                int depth = this.depth;
                string str = null;
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
                    this.depth++;
                    this.sb.Append("(");
                    this.NewLine();
                }
                this.Visit(alias.Node);
                if (flag)
                {
                    this.NewLine();
                    this.sb.Append(")");
                    this.depth = depth;
                }
                if ((!this.suppressedAliases.Contains(alias) && (str != null)) && (name != str))
                {
                    if (Mode == SqlProvider.ProviderMode.Oracle || Mode == SqlProvider.ProviderMode.OdpOracle)
                        sb.Append(" ");
                    else
                        this.sb.Append(" AS ");
                    this.WriteName(str);
                }
                return alias;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                this.sb.Append("AREF(");
                this.WriteAliasName(aref.Alias);
                this.sb.Append(")");
                return aref;
            }

            internal override SqlStatement VisitAssign(SqlAssign sa)
            {
                this.sb.Append("SET ");
                this.Visit(sa.LValue);
                this.sb.Append(" = ");
                this.Visit(sa.RValue);
                return sa;
            }

            internal override SqlExpression VisitBetween(SqlBetween between)
            {
                this.VisitWithParens(between.Expression, between);
                this.sb.Append(" BETWEEN ");
                this.Visit(between.Start);
                this.sb.Append(" AND ");
                this.Visit(between.End);
                return between;
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
                this.sb.Append(this.GetOperator(bo.NodeType));
                this.sb.Append(" ");
                this.VisitWithParens(bo.Right, bo);
                return bo;
            }

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
                            this.NewLine();
                            this.NewLine();
                        }
                    }
                    num++;
                }
                return block;
            }

            internal override SqlExpression VisitCast(SqlUnary c)
            {
                this.sb.Append("CAST(");
                this.Visit(c.Operand);
                this.sb.Append(" AS ");
                QueryFormatOptions none = QueryFormatOptions.None;
                if (c.Operand.SqlType.CanSuppressSizeForConversionToString)
                {
                    none = QueryFormatOptions.SuppressSize;
                }
                this.sb.Append(c.SqlType.ToQueryString(none));
                this.sb.Append(")");
                return c;
            }

            internal override SqlExpression VisitClientArray(SqlClientArray scar)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("ClientArray");
                }
                this.sb.Append("new []{");
                int num = 0;
                int count = scar.Expressions.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.Visit(scar.Expressions[num]);
                    num++;
                }
                this.sb.Append("}");
                return scar;
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("ClientCase");
                }
                this.depth++;
                this.NewLine();
                this.sb.Append("(CASE");
                this.depth++;
                if (c.Expression != null)
                {
                    this.sb.Append(" ");
                    this.Visit(c.Expression);
                }
                int num = 0;
                int count = c.Whens.Count;
                while (num < count)
                {
                    SqlClientWhen when = c.Whens[num];
                    if ((num == (count - 1)) && (when.Match == null))
                    {
                        this.NewLine();
                        this.sb.Append("ELSE ");
                        this.Visit(when.Value);
                    }
                    else
                    {
                        this.NewLine();
                        this.sb.Append("WHEN ");
                        this.Visit(when.Match);
                        this.sb.Append(" THEN ");
                        this.Visit(when.Value);
                    }
                    num++;
                }
                this.depth--;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitClientParameter(SqlClientParameter cp)
            {
                object obj2;
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("ClientParameter");
                }
                this.sb.Append("client-parameter(");
                try
                {
                    obj2 = cp.Accessor.Compile().DynamicInvoke(new object[1]);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
                this.sb.Append(obj2);
                this.sb.Append(")");
                return cp;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("ClientQuery");
                }
                this.sb.Append("client(");
                int num = 0;
                int count = cq.Arguments.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.Visit(cq.Arguments[num]);
                    num++;
                }
                this.sb.Append("; ");
                this.Visit(cq.Query);
                this.sb.Append(")");
                return cq;
            }

            internal override SqlExpression VisitColumn(SqlColumn c)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("Column");
                }
                this.sb.Append("COLUMN(");
                if (c.Expression != null)
                {
                    this.Visit(c.Expression);
                }
                else
                {
                    string name = null;
                    if (c.Alias != null)
                    {
                        if (c.Alias.Name == null)
                        {
                            if (!this.names.TryGetValue(c.Alias, out name))
                            {
                                name = "A" + this.names.Count;
                                this.names[c.Alias] = name;
                            }
                        }
                        else
                        {
                            name = c.Alias.Name;
                        }
                    }
                    this.sb.Append(name);
                    this.sb.Append(".");
                    this.sb.Append(c.Name);
                }
                this.sb.Append(")");
                return c;
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
                if (alias != null)
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
                if ((!this.suppressedAliases.Contains(key.Alias) && (str != null)) && (str.Length != 0))
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

            protected internal virtual void VisitCrossJoinList(SqlNode node)
            {
                var join = node as SqlJoin;
                if (join != null)
                {
                    this.VisitCrossJoinList(join.Left);
                    this.sb.Append(", ");
                    this.VisitCrossJoinList(join.Right);
                }
                else
                {
                    this.Visit(node);
                }
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                sb.Append("DELETE FROM ");
                suppressedAliases.Add(sd.Select.From);
                Visit(sd.Select.From);
                if (sd.Select.Where != null)
                {
                    sb.Append(" WHERE ");
                    Visit(sd.Select.Where);
                }
                suppressedAliases.Remove(sd.Select.From);
                return sd;
            }

            internal override SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt)
            {
                if (this.isDebugMode)
                {
                    this.sb.Append("DTYPE(");
                }
                base.VisitDiscriminatedType(dt);
                if (this.isDebugMode)
                {
                    this.sb.Append(")");
                }
                return dt;
            }

            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
            {
                if (this.isDebugMode)
                {
                    this.sb.Append("DISCO(");
                }
                base.VisitDiscriminatorOf(dof);
                if (this.isDebugMode)
                {
                    this.sb.Append(")");
                }
                return dof;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("Element");
                }
                int depth = this.depth;
                this.depth++;
                this.sb.Append("ELEMENT(");
                this.NewLine();
                this.Visit(elem.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = depth;
                return elem;
            }

            internal override SqlExpression VisitExists(SqlSubSelect sqlExpr)
            {
                int oldDepth = depth;
                this.depth++;
                this.sb.Append("EXISTS(");
                this.NewLine();
                this.Visit(sqlExpr.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = oldDepth;
                return sqlExpr;
            }

            internal override SqlExpression VisitExprSet(SqlExprSet xs)
            {
                if (this.isDebugMode)
                {
                    this.sb.Append("ES(");
                    int num = 0;
                    int count = xs.Expressions.Count;
                    while (num < count)
                    {
                        if (num > 0)
                        {
                            this.sb.Append(", ");
                        }
                        this.Visit(xs.Expressions[num]);
                        num++;
                    }
                    this.sb.Append(")");
                    return xs;
                }
                this.Visit(xs.GetFirstExpression());
                return xs;
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                if (Mode == SqlProvider.ProviderMode.OdpOracle || Mode == SqlProvider.ProviderMode.Oracle)
                    if (fc.Arguments.Count == 0)
                        fc.Brackets = false;

                if (fc.Name.Contains("."))
                    WriteName(fc.Name);
                else
                    sb.Append(fc.Name);

                if (fc.Brackets)
                    sb.Append("(");
                else
                    sb.Append(" ");

                int num = 0;
                int count = fc.Arguments.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        if (fc.Comma)
                            this.sb.Append(", ");
                        else
                            sb.Append(" ");
                    }
                    this.Visit(fc.Arguments[num]);
                    num++;
                }

                if (fc.Brackets)
                    this.sb.Append(")");
                return fc;
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("Grouping");
                }
                this.sb.Append("Group(");
                this.Visit(g.Key);
                this.sb.Append(", ");
                this.Visit(g.Group);
                this.sb.Append(")");
                return g;
            }

            internal override SqlExpression VisitIn(SqlIn sin)
            {
                this.VisitWithParens(sin.Expression, sin);
                this.sb.Append(" IN (");
                int num = 0;
                int count = sin.Values.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.Visit(sin.Values[num]);
                    num++;
                }
                this.sb.Append(")");
                return sin;
            }

            internal override SqlStatement VisitInsert(SqlInsert si)
            {
                if (si.OutputKey != null)
                {
                    this.sb.Append("DECLARE @output TABLE(");
                    this.WriteName(si.OutputKey.Name);
                    this.sb.Append(" ");
                    this.sb.Append(si.OutputKey.SqlType.ToQueryString());
                    this.sb.Append(")");
                    this.NewLine();
                    if (si.OutputToLocal)
                    {
                        this.sb.Append("DECLARE @id ");
                        this.sb.Append(si.OutputKey.SqlType.ToQueryString());
                        this.NewLine();
                    }
                }
                this.sb.Append("INSERT INTO ");
                this.Visit(si.Table);
                if (si.Row.Columns.Count != 0)
                {
                    this.sb.Append("(");
                    int num = 0;
                    int count = si.Row.Columns.Count;
                    while (num < count)
                    {
                        if (num > 0)
                        {
                            this.sb.Append(", ");
                        }
                        this.WriteName(si.Row.Columns[num].Name);
                        num++;
                    }
                    this.sb.Append(")");
                }
                if (si.OutputKey != null)
                {
                    this.NewLine();
                    this.sb.Append("OUTPUT INSERTED.");
                    this.WriteName(si.OutputKey.Name);
                    if (si.OutputToLocal)
                    {
                        this.sb.Append(" INTO @output");
                    }
                }
                if (si.Row.Columns.Count == 0)
                {
                    this.sb.Append(" DEFAULT VALUES");
                }
                else
                {
                    this.NewLine();
                    this.sb.Append("VALUES (");
                    if (this.isDebugMode && (si.Row.Columns.Count == 0))
                    {
                        this.Visit(si.Expression);
                    }
                    else
                    {
                        int num3 = 0;
                        int num4 = si.Row.Columns.Count;
                        while (num3 < num4)
                        {
                            if (num3 > 0)
                            {
                                this.sb.Append(", ");
                            }
                            this.Visit(si.Row.Columns[num3].Expression);
                            num3++;
                        }
                    }
                    this.sb.Append(")");
                }
                if (si.OutputKey != null)
                {
                    this.NewLine();
                    if (si.OutputToLocal)
                    {
                        this.sb.Append("SELECT @id = ");
                        this.sb.Append(si.OutputKey.Name);
                        this.sb.Append(" FROM @output");
                        return si;
                    }
                    this.sb.Append("SELECT ");
                    this.WriteName(si.OutputKey.Name);
                    this.sb.Append(" FROM @output");
                }
                return si;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                //if (this.Mode == SqlProvider.ProviderMode.Access)
                //    this.sb.Append("(");

                this.Visit(join.Left);
                this.NewLine();
                switch (join.JoinType)
                {
                    case SqlJoinType.Cross:
                        this.sb.Append("CROSS JOIN ");
                        break;

                    case SqlJoinType.Inner:
                        this.sb.Append("INNER JOIN ");
                        break;

                    case SqlJoinType.LeftOuter:
                        this.sb.Append("LEFT OUTER JOIN ");
                        break;

                    case SqlJoinType.CrossApply:
                        this.sb.Append("CROSS APPLY ");
                        break;

                    case SqlJoinType.OuterApply:
                        this.sb.Append("OUTER APPLY ");
                        break;
                }
                SqlJoin right = join.Right as SqlJoin;
                if ((right == null) ||
                    (((right.JoinType == SqlJoinType.Cross) && (join.JoinType != SqlJoinType.CrossApply)) &&
                     (join.JoinType != SqlJoinType.OuterApply)))
                {
                    this.Visit(join.Right);
                }
                else
                {
                    this.VisitJoinSource(join.Right);
                }
                if (join.Condition != null)
                {
                    this.sb.Append(" ON ");
                    this.Visit(join.Condition);
                    //return join;
                }
                else if (RequiresOnCondition(join.JoinType))
                {
                    this.sb.Append(" ON 1=1 ");
                }

                //if (Mode == SqlProvider.ProviderMode.Access)
                //    this.sb.Append(")");

                return join;
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("JoinedCollection");
                }
                this.sb.Append("big-join(");
                this.Visit(jc.Expression);
                this.sb.Append(", ");
                this.Visit(jc.Count);
                this.sb.Append(")");
                return jc;
            }

            internal void VisitJoinSource(SqlSource src)
            {
                if (src.NodeType == SqlNodeType.Join)
                {
                    this.depth++;
                    this.sb.Append("(");
                    this.Visit(src);
                    this.sb.Append(")");
                    this.depth--;
                }
                else
                {
                    this.Visit(src);
                }
            }

            internal override SqlExpression VisitLift(SqlLift lift)
            {
                this.Visit(lift.Expression);
                return lift;
            }

            internal override SqlExpression VisitLike(SqlLike like)
            {
                this.VisitWithParens(like.Expression, like);
                this.sb.Append(" LIKE ");
                this.Visit(like.Pattern);
                if (like.Escape != null)
                {
                    this.sb.Append(" ESCAPE ");
                    this.Visit(like.Escape);
                }
                return like;
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("Link");
                }
                if (link.Expansion != null)
                {
                    this.sb.Append("LINK(");
                    this.Visit(link.Expansion);
                    this.sb.Append(")");
                    return link;
                }
                this.sb.Append("LINK(");
                int num = 0;
                int count = link.KeyExpressions.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.Visit(link.KeyExpressions[num]);
                    num++;
                }
                this.sb.Append(")");
                return link;
            }

            protected override SqlNode VisitMember(SqlMember m)
            {
                this.Visit(m.Expression);
                this.sb.Append(".");
                this.sb.Append(m.Member.Name);
                return m;
            }

            internal override SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma)
            {
                throw Error.InvalidFormatNode("MemberAssign");
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("MethodCall");
                }
                if (mc.Method.IsStatic)
                {
                    this.sb.Append(mc.Method.DeclaringType);
                }
                else
                {
                    this.Visit(mc.Object);
                }
                this.sb.Append(".");
                this.sb.Append(mc.Method.Name);
                this.sb.Append("(");
                int num = 0;
                int count = mc.Arguments.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.Visit(mc.Arguments[num]);
                    num++;
                }
                this.sb.Append(")");
                return mc;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("Multiset");
                }
                int depth = this.depth;
                this.depth++;
                this.sb.Append("MULTISET(");
                this.NewLine();
                this.Visit(sms.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = depth;
                return sms;
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("New");
                }
                this.sb.Append("new ");
                this.sb.Append(sox.ClrType.Name);
                this.sb.Append("{ ");
                int num = 0;
                int count = sox.Args.Count;
                while (num < count)
                {
                    SqlExpression node = sox.Args[num];
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.sb.Append(sox.ArgMembers[num].Name);
                    this.sb.Append(" = ");
                    this.Visit(node);
                    num++;
                }
                int num3 = 0;
                int num4 = sox.Members.Count;
                while (num3 < num4)
                {
                    SqlMemberAssign assign = sox.Members[num3];
                    if (num3 > 0)
                    {
                        this.sb.Append(", ");
                    }
                    if (this.InferName(assign.Expression, null) != assign.Member.Name)
                    {
                        this.sb.Append(assign.Member.Name);
                        this.sb.Append(" = ");
                    }
                    this.Visit(assign.Expression);
                    num3++;
                }
                this.sb.Append(" }");
                return sox;
            }

            internal override SqlExpression VisitNop(SqlNop nop)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("NOP");
                }
                this.sb.Append("NOP()");
                return nop;
            }

            internal override SqlExpression VisitObjectType(SqlObjectType ot)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("ObjectType");
                }
                this.sb.Append("(");
                base.VisitObjectType(ot);
                this.sb.Append(").GetType()");
                return ot;
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("OptionalValue");
                }
                this.sb.Append("opt(");
                this.Visit(sov.HasValue);
                this.sb.Append(", ");
                this.Visit(sov.Value);
                this.sb.Append(")");
                return sov;
            }

            internal override SqlExpression VisitParameter(SqlParameter p)
            {
                sb.Append(p.Name);
                return p;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                int num = 0;
                int count = row.Columns.Count;
                while (num < count)
                {
                    SqlColumn key = row.Columns[num];
                    if (num > 0)
                    {
                        sb.Append(", ");
                    }
                    Visit(key.Expression);
                    string name = key.Name;
                    string str2 = InferName(key.Expression, null);
                    if (name == null)
                    {
                        name = str2;
                    }
                    if ((name == null) && !this.names.TryGetValue(key, out name))
                    {
                        name = "C" + this.names.Count;
                        this.names[key] = name;
                    }
                    if (Mode == SqlProvider.ProviderMode.Oracle || Mode == SqlProvider.ProviderMode.OdpOracle)
                    {
                        if (str2 != null)
                            str2 = sqlIdentifier.UnquoteIdentifier(str2);
                    }
                    if (!string.IsNullOrEmpty(name) && name != str2)
                    {
                        if (Mode == SqlProvider.ProviderMode.Oracle || Mode == SqlProvider.ProviderMode.OdpOracle)
                            sb.Append(" ");
                        else
                            sb.Append(" AS ");

                        this.WriteName(name);
                    }
                    num++;
                }
                return row;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                this.sb.Append("ROW_NUMBER() OVER (ORDER BY ");
                int num = 0;
                int count = rowNumber.OrderBy.Count;
                while (num < count)
                {
                    SqlOrderExpression expression = rowNumber.OrderBy[num];
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.Visit(expression.Expression);
                    if (expression.OrderType == SqlOrderType.Descending)
                    {
                        this.sb.Append(" DESC");
                    }
                    num++;
                }
                this.sb.Append(")");
                return rowNumber;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                int depth = this.depth;
                this.depth++;
                if (this.isDebugMode)
                {
                    this.sb.Append("SCALAR");
                }
                this.sb.Append("(");
                this.NewLine();
                this.Visit(ss.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = depth;
                return ss;
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                this.depth++;
                this.NewLine();
                this.sb.Append("(CASE ");
                this.depth++;
                int num = 0;
                int count = c.Whens.Count;
                while (num < count)
                {
                    SqlWhen when = c.Whens[num];
                    this.NewLine();
                    this.sb.Append("WHEN ");
                    this.Visit(when.Match);
                    this.sb.Append(" THEN ");
                    this.Visit(when.Value);
                    num++;
                }
                if (c.Else != null)
                {
                    this.NewLine();
                    this.sb.Append("ELSE ");
                    this.Visit(c.Else);
                }
                this.depth--;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth--;
                return c;
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
                        this.sb.Append("TOP ");
                        if (this.parenthesizeTop)
                        {
                            this.sb.Append("(");
                        }
                        this.Visit(ss.Top);
                        if (this.parenthesizeTop)
                        {
                            this.sb.Append(")");
                        }
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

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("Shared");
                }
                this.sb.Append("SHARED(");
                this.Visit(shared.Expression);
                this.sb.Append(")");
                return shared;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("SharedRef");
                }
                this.sb.Append("SHAREDREF(");
                this.Visit(sref.SharedExpression.Expression);
                this.sb.Append(")");
                return sref;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                this.depth++;
                this.NewLine();
                this.sb.Append("(CASE");
                this.depth++;
                if (c.Expression != null)
                {
                    this.sb.Append(" ");
                    this.Visit(c.Expression);
                }
                int num = 0;
                int count = c.Whens.Count;
                while (num < count)
                {
                    SqlWhen when = c.Whens[num];
                    if ((num == (count - 1)) && (when.Match == null))
                    {
                        this.NewLine();
                        this.sb.Append("ELSE ");
                        this.Visit(when.Value);
                    }
                    else
                    {
                        this.NewLine();
                        this.sb.Append("WHEN ");
                        this.Visit(when.Match);
                        this.sb.Append(" THEN ");
                        this.Visit(when.Value);
                    }
                    num++;
                }
                this.depth--;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("SIMPLE");
                }
                this.sb.Append("SIMPLE(");
                base.VisitSimpleExpression(simple);
                this.sb.Append(")");
                return simple;
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                sb.Append("EXEC ");
                if (spc.Function.Method.ReturnType != typeof(void))
                    sb.Append("@RETURN_VALUE = ");

                WriteName(spc.Function.MappedName);
                sb.Append(" ");
                int count = spc.Function.Parameters.Count;
                for (int i = 0; i < count; i++)
                {
                    MetaParameter parameter = spc.Function.Parameters[i];
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    WriteRecordVariableName(parameter.MappedName);
                    sb.Append(" = ");
                    Visit(spc.Arguments[i]);
                    if (parameter.Parameter.IsOut || parameter.Parameter.ParameterType.IsByRef)
                    {
                        sb.Append(" OUTPUT");
                    }
                }
                if (spc.Arguments.Count > count)
                {
                    if (count > 0)
                    {
                        this.sb.Append(", ");
                    }
                    this.WriteRecordVariableName(spc.Function.ReturnParameter.MappedName);
                    this.sb.Append(" = ");
                    this.Visit(spc.Arguments[count]);
                    this.sb.Append(" OUTPUT");
                }
                return spc;

            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                string name = tab.Name;
                this.WriteName(name);
                return tab;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                return this.VisitFunctionCall(fc);
            }

            internal override SqlExpression VisitTreat(SqlUnary t)
            {
                this.sb.Append("TREAT(");
                this.Visit(t.Operand);
                this.sb.Append(" AS ");
                this.FormatType(t.SqlType);
                this.sb.Append(")");
                return t;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase c)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("TypeCase");
                }
                this.depth++;
                this.NewLine();
                this.sb.Append("(CASE");
                this.depth++;
                if (c.Discriminator != null)
                {
                    this.sb.Append(" ");
                    this.Visit(c.Discriminator);
                }
                int num = 0;
                int count = c.Whens.Count;
                while (num < count)
                {
                    SqlTypeCaseWhen when = c.Whens[num];
                    if ((num == (count - 1)) && (when.Match == null))
                    {
                        this.NewLine();
                        this.sb.Append("ELSE ");
                        this.Visit(when.TypeBinding);
                    }
                    else
                    {
                        this.NewLine();
                        this.sb.Append("WHEN ");
                        this.Visit(when.Match);
                        this.sb.Append(" THEN ");
                        this.Visit(when.TypeBinding);
                    }
                    num++;
                }
                this.depth--;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                switch (uo.NodeType)
                {
                    case SqlNodeType.Avg:
                    case SqlNodeType.ClrLength:
                    case SqlNodeType.Count:
                    case SqlNodeType.Covar:
                    case SqlNodeType.LongCount:
                    case SqlNodeType.Min:
                    case SqlNodeType.Max:
                    case SqlNodeType.Sum:
                    case SqlNodeType.Stddev:
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        this.sb.Append("(");
                        if (uo.Operand == null)
                        {
                            this.sb.Append("*");
                        }
                        else
                        {
                            this.Visit(uo.Operand);
                        }
                        this.sb.Append(")");
                        return uo;

                    case SqlNodeType.BitNot:
                    case SqlNodeType.Negate:
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        this.VisitWithParens(uo.Operand, uo);
                        return uo;

                    case SqlNodeType.Convert:
                        {
                            return TranslateConverter(uo);
                        }
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                        this.VisitWithParens(uo.Operand, uo);
                        this.sb.Append(" ");
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        return uo;

                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        this.sb.Append(" ");
                        this.VisitWithParens(uo.Operand, uo);
                        return uo;

                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                        this.Visit(uo.Operand);
                        return uo;
                }
                throw Error.InvalidFormatNode(uo.NodeType);
            }

            protected virtual SqlExpression TranslateConverter(SqlUnary uo)
            {
                sb.Append("CONVERT(");
                QueryFormatOptions none = QueryFormatOptions.None;
                if (uo.Operand.SqlType.CanSuppressSizeForConversionToString)
                {
                    none = QueryFormatOptions.SuppressSize;
                }
                sb.Append(uo.SqlType.ToQueryString(none));
                sb.Append(",");
                Visit(uo.Operand);
                sb.Append(")");
                return uo;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                this.sb.Append("(");
                int depth = this.depth;
                this.depth++;
                this.NewLine();
                this.Visit(su.Left);
                this.NewLine();
                this.sb.Append("UNION");
                if (su.All)
                {
                    this.sb.Append(" ALL");
                }
                this.NewLine();
                this.Visit(su.Right);
                this.NewLine();
                this.sb.Append(")");
                this.depth = depth;
                return su;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate su)
            {
                if (su.IsInsert)
                    return VisitInsert(su);

                this.sb.Append("UPDATE ");
                this.suppressedAliases.Add(su.Select.From);
                this.Visit(su.Select.From);
                this.NewLine();
                this.sb.Append("SET ");
                int num = 0;
                int count = su.Assignments.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        this.sb.Append(", ");
                    }
                    SqlAssign assign = su.Assignments[num];
                    this.Visit(assign.LValue);
                    this.sb.Append(" = ");
                    this.Visit(assign.RValue);
                    num++;
                }
                if (su.Select.Where != null)
                {
                    this.NewLine();
                    this.sb.Append("WHERE ");
                    this.Visit(su.Select.Where);
                }
                this.suppressedAliases.Remove(su.Select.From);
                return su;
            }

            SqlStatement VisitInsert(SqlUpdate su)
            {
                this.sb.Append("INSERT INTO ");
                this.suppressedAliases.Add(su.Select.From);
                this.Visit(su.Select.From);
                //this.NewLine();
                this.sb.Append("(");
                int num = 0;
                int count = su.Assignments.Count;
                while (num < count)
                {
                    if (num > 0)
                        this.sb.Append(", ");

                    SqlAssign assign = su.Assignments[num];
                    this.Visit(assign.LValue);
                    num++;
                }
                this.sb.Append(")");

                this.NewLine();
                this.sb.Append("VALUES ");
                sb.Append("(");
                num = 0;
                while (num < count)
                {
                    if (num > 0)
                        this.sb.Append(", ");

                    SqlAssign assign = su.Assignments[num];
                    this.Visit(assign.RValue);
                    num++;
                }
                sb.Append(")");

                return su;
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc)
            {
                this.sb.Append(suc.Name);
                return suc;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                if (suq.Arguments.Count > 0)
                {
                    StringBuilder sb = this.sb;
                    this.sb = new StringBuilder();
                    object[] args = new object[suq.Arguments.Count];
                    int index = 0;
                    int length = args.Length;
                    while (index < length)
                    {
                        this.Visit(suq.Arguments[index]);
                        args[index] = this.sb.ToString();
                        this.sb.Length = 0;
                        index++;
                    }
                    this.sb = sb;
                    this.sb.Append(string.Format(CultureInfo.InvariantCulture, suq.QueryText, args));
                    return suq;
                }
                this.sb.Append(suq.QueryText);
                return suq;
            }

            internal override SqlExpression VisitUserRow(SqlUserRow row)
            {
                if (!this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("UserRow");
                }
                return row;
            }

            internal override SqlExpression VisitValue(SqlValue sqlValue)
            {
                if (sqlValue.IsClientSpecified && !this.isDebugMode)
                {
                    throw Error.InvalidFormatNode("Value");
                }
                FormatValue(sqlValue.Value);
                return sqlValue;
            }

            internal override SqlExpression VisitVariable(SqlVariable v)
            {
                //if (v.Name.StartsWith(SqlIdentifier.ParameterPrefix))
                sb.Append(v.Name);
                //else
                //sb.Append(SqlIdentifier.QuoteCompoundIdentifier(v.Name));
                return v;
            }

            protected internal virtual void VisitWithParens(SqlNode node, SqlNode outer)
            {
                if (node == null)
                {
                    return;
                }
                SqlNodeType nodeType = node.NodeType;
                if (nodeType <= SqlNodeType.Member)
                {
                    switch (nodeType)
                    {
                        case SqlNodeType.FunctionCall:
                        case SqlNodeType.Member:
                        case SqlNodeType.ColumnRef:
                            goto Label_0099;

                        case SqlNodeType.Add:
                        case SqlNodeType.And:
                        case SqlNodeType.BitAnd:
                        case SqlNodeType.BitNot:
                        case SqlNodeType.BitOr:
                        case SqlNodeType.BitXor:
                            goto Label_00A2;

                        case SqlNodeType.Alias:
                        case SqlNodeType.AliasRef:
                        case SqlNodeType.Assign:
                        case SqlNodeType.Avg:
                        case SqlNodeType.Between:
                            goto Label_00B9;
                    }
                    goto Label_00B9;
                }
                if (nodeType <= SqlNodeType.Parameter)
                {
                    switch (nodeType)
                    {
                        case SqlNodeType.Not:
                        case SqlNodeType.Not2V:
                        case SqlNodeType.Or:
                        case SqlNodeType.Mul:
                            goto Label_00A2;

                        case SqlNodeType.Nop:
                        case SqlNodeType.ObjectType:
                        case SqlNodeType.OptionalValue:
                            goto Label_00B9;

                        case SqlNodeType.OuterJoinedValue:
                        case SqlNodeType.Parameter:
                            goto Label_0099;
                    }
                    goto Label_00B9;
                }
                if ((nodeType != SqlNodeType.TableValuedFunctionCall) && (nodeType != SqlNodeType.Value))
                {
                    goto Label_00B9;
                }
            Label_0099:
                this.Visit(node);
                return;
            Label_00A2:
                if (outer.NodeType == node.NodeType)
                {
                    this.Visit(node);
                    return;
                }
            Label_00B9:
                if (this.Mode == SqlProvider.ProviderMode.Access && nodeType == SqlNodeType.Variable)
                {
                    this.Visit(node);
                }
                else
                {
                    this.sb.Append("(");
                    this.Visit(node);
                    this.sb.Append(")");
                }

            }

            private void WriteAliasName(SqlAlias alias)
            {
                string name = null;
                if (alias.Name == null)
                {
                    if (!this.names.TryGetValue(alias, out name))
                    {
                        name = "A" + this.names.Count;
                        this.names[alias] = name;
                    }
                }
                else
                {
                    name = alias.Name;
                }
                this.WriteName(name);
            }

            protected virtual void WriteName(string name)
            {
                this.sb.Append(SqlIdentifier.QuoteCompoundIdentifier(name));
            }

            public SqlProvider.ProviderMode Mode
            {
                get;
                set;
            }

            private void WriteRecordVariableName(string name)
            {
                WriteVariableName(name);
            }

            protected virtual void WriteVariableName(string name)
            {
                if (name.StartsWith(SqlIdentifier.ParameterPrefix, StringComparison.Ordinal))
                {
                    this.sb.Append(SqlIdentifier.QuoteCompoundIdentifier(name));
                }
                else
                {
                    this.sb.Append(SqlIdentifier.QuoteCompoundIdentifier("@" + name));
                }
            }
        }


    }
}