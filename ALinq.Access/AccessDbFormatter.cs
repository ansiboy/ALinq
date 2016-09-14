using System;
using System.Collections.Generic;
using ALinq.Mapping;
using System.Diagnostics;
using ALinq.SqlClient;

namespace ALinq.Access
{
    class AccessDbFormatter : DbFormatter
    {
        internal class Visitor : SqlFormatter.Visitor
        {
            internal readonly IList<string> parameterNames;

            public Visitor()
            {
                parameterNames = new List<string>();
            }

            internal override string GetBoolValue(bool value)
            {
                if (value)
                    return "true";
                return "false";
            }

            public override string GetOperator(SqlNodeType nt)
            {
                if (nt == SqlNodeType.LongCount)
                    return "COUNT";
                return base.GetOperator(nt);
            }

            internal override SqlExpression VisitParameter(SqlParameter p)
            {
                if (!parameterNames.Contains(p.Name))
                    parameterNames.Add(p.Name);

                return base.VisitParameter(p);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                if (uo.NodeType == SqlNodeType.Max || uo.NodeType == SqlNodeType.Min ||
                    uo.NodeType == SqlNodeType.Avg || uo.NodeType == SqlNodeType.Sum)
                {
                    Debug.Assert(uo.ClrType != null);

                    sb.Append("IIF(");

                    sb.Append(GetOperator(uo.NodeType));
                    sb.Append("(");
                    if (uo.Operand == null)
                        sb.Append("*");
                    else
                        Visit(uo.Operand);
                    sb.Append(")");

                    var defaultValue = TypeSystem.GetDefaultValue(uo.ClrType);
                    if (defaultValue is DateTime)
                        sb.Append(string.Format(" IS NULL, '{0}', ", ((DateTime)defaultValue).ToShortDateString()));
                    else if (defaultValue is string)
                        sb.Append(string.Format(" IS NULL, '{0}', ", defaultValue));
                    else
                        sb.Append(string.Format(" IS NULL, {0}, ", defaultValue));

                    sb.Append(GetOperator(uo.NodeType));
                    sb.Append("(");
                    if (uo.Operand == null)
                        sb.Append("*");
                    else
                        Visit(uo.Operand);
                    sb.Append(")");

                    sb.Append(")");
                    return uo;

                }
                return base.VisitUnaryOperator(uo);
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                this.depth++;
                int num = 0;
                int count;
                SqlNode elseNode;
                if (c.Else != null)
                {
                    elseNode = c.Else;
                    count = c.Whens.Count;
                }
                else
                {
                    elseNode = c.Whens[c.Whens.Count - 1].Match;
                    count = c.Whens.Count - 1;
                }
                this.NewLine();
                while (num < count)
                {
                    SqlWhen when = c.Whens[num];
                    this.sb.Append("IIF(");
                    this.Visit(when.Match);
                    this.sb.Append(" , ");
                    this.Visit(when.Value);
                    this.sb.Append(" , ");
                    num++;
                }
                if (c.Else != null)
                {
                    this.Visit(elseNode);
                }
                for (int i = 0; i < count; i++)
                {
                    sb.Append(")");
                }
                this.NewLine();
                this.depth--;
                return c;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                this.depth++;
                int num = 0;
                Debug.Assert(c.Whens.Count > 1);
                SqlWhen elseNode = c.Whens[c.Whens.Count - 1];
                int count = c.Whens.Count - 1;
                this.NewLine();
                while (num < count)
                {
                    SqlWhen when = c.Whens[num];
                    this.sb.Append("IIF(");
                    Visit(c.Expression);
                    sb.Append(" = ");
                    this.Visit(when.Match);
                    this.sb.Append(" , ");
                    this.Visit(when.Value);
                    this.sb.Append(" , ");
                    num++;
                }
                Debug.Assert(elseNode != null);
                this.Visit(elseNode.Value);

                for (int i = 0; i < count; i++)
                {
                    sb.Append(")");
                }
                this.NewLine();
                this.depth--;
                return c;
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                sb.Append("EXECUTE ");
                sb.Append(spc.Function.MappedName);
                sb.Append(" ");
                int count = spc.Function.Parameters.Count;
                for (int i = 0; i < count; i++)
                {
                    MetaParameter parameter = spc.Function.Parameters[i];
                    sb.Append(parameter.MappedName);
                    if (i < count - 1)
                    {
                        sb.Append(", ");
                    }
                    parameterNames.Add(((SqlParameter)spc.Arguments[i]).Name);
                }
                return spc;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                if (bo.NodeType == SqlNodeType.Coalesce)
                {
                    sb.Append("IIF(");
                    Visit(bo.Left);
                    sb.Append(" IS NULL, ");
                    Visit(bo.Right);
                    sb.Append(", ");
                    Visit(bo.Left);
                    sb.Append(")");
                    return bo;
                }
                VisitWithParens(bo.Left, bo);
                sb.Append(" ");
                sb.Append(GetOperator(bo.NodeType));
                sb.Append(" ");
                VisitWithParens(bo.Right, bo);
                return bo;
            }

            protected override SqlExpression TranslateConverter(SqlUnary uo)
            {
                if (uo.ClrType == typeof(bool))
                {
                    sb.Append("CBool(");
                    Visit(uo.Operand);
                    sb.Append(")");
                }
                else if (uo.ClrType == typeof(decimal))
                {
                    sb.Append("CDec(");
                    Visit(uo.Operand);
                    sb.Append(")");
                }
                else if (uo.ClrType == typeof(sbyte))
                {
                    sb.Append("CByte(");
                    Visit(uo.Operand);
                    sb.Append(")");
                }
                else if (uo.ClrType == typeof(char))
                {
                    sb.Append("CStr(");
                    Visit(uo.Operand);
                    sb.Append(")");
                }
                else
                    if (uo.ClrType == typeof(DateTime))
                    {
                        sb.Append("CDate(");
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else if (uo.ClrType == typeof(double))
                    {
                        sb.Append("CDbl(");
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else if (uo.ClrType == typeof(byte) || uo.ClrType == typeof(int))
                    {
                        sb.Append("CInt(");
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else if (uo.ClrType == typeof(long))
                    {
                        sb.Append("CLng(");
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else if (uo.ClrType == typeof(float))
                    {
                        sb.Append("CSng(");
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else if (uo.ClrType == typeof(string))
                    {
                        sb.Append("CStr(");
                        Visit(uo.Operand);
                        sb.Append(")");
                    }
                    else
                    {
                        Visit(uo.Operand);
                    }
                return uo;
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                if (fc.Name == "IIF")
                {
                    if (fc.Name.Contains("."))
                        WriteName(fc.Name);
                    else
                        sb.Append(fc.Name);

                    sb.Append("(");
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
                        if (num == 0)
                            sb.Append(" IS NULL");
                        num++;
                    }
                    this.sb.Append(")");
                    return fc;
                }
                return base.VisitFunctionCall(fc);
            }

            void OutJoinName(SqlJoin join)
            {
                switch (join.JoinType)
                {
                    case SqlJoinType.Cross:
                        if (this.Mode == SqlProvider.ProviderMode.Access)
                            this.sb.Append(",");
                        else
                        {
                            this.sb.Append("CROSS JOIN ");
                        }
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
                        this.NewLine();
                        this.sb.Append("OUTER APPLY ");
                        break;
                }
            }

            void OutJoinCondition(SqlJoin join)
            {
                if (join.Condition != null)
                {
                    this.sb.Append(" ON ");
                    this.Visit(join.Condition);
                }
                else if (RequiresOnCondition(join.JoinType))
                {
                    this.sb.Append(" ON 1=1 ");
                }
            }

            int joinDepth = 0;
            internal override SqlSource VisitJoin(SqlJoin join)
            {
                if (joinDepth > 0 && !IsCrossJoin(join))
                    this.sb.Append("(");

                joinDepth = joinDepth + 1;

                this.Visit(join.Left);

                SqlJoin right = join.Right as SqlJoin;
                if ((right == null) ||
                    (((right.JoinType == SqlJoinType.Cross) && (join.JoinType != SqlJoinType.CrossApply)) &&
                     (join.JoinType != SqlJoinType.OuterApply)))
                {
                    if (join.JoinType != SqlJoinType.Cross)
                        this.NewLine();

                    OutJoinName(join);
                    this.Visit(join.Right);
                    OutJoinCondition(join);

                }
                else
                {
                    OutJoinName(join);
                    this.VisitJoinSource(join.Right);
                    OutJoinCondition(join);
                }

                joinDepth = joinDepth - 1;

                if (joinDepth > 0 && !IsCrossJoin(join))
                    this.sb.Append(")");

                return join;

            }

            bool IsCrossJoin(SqlNode sqlNode)
            {
                if (sqlNode.NodeType != SqlNodeType.Join)
                    return false;
                if (((SqlJoin)sqlNode).JoinType != SqlJoinType.Cross)
                    return false;

                return true;
            }

            SqlNode VisitJoinRight(SqlNode join)
            {
                this.sb.Append("(");
                var result = base.Visit(join);
                this.sb.Append(")");
                return result;
            }
        }

        public AccessDbFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {
        }

        internal override SqlFormatter.Visitor CreateVisitor()
        {
            return new Visitor();
        }
    }
}
