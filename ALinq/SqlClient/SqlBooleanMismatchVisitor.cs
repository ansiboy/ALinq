namespace ALinq.SqlClient
{
    internal abstract class SqlBooleanMismatchVisitor : SqlVisitor
    {
        // Methods
        internal SqlBooleanMismatchVisitor()
        {
        }

        internal abstract SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression);
        internal abstract SqlExpression ConvertValueToPredicate(SqlExpression valueExpression);

        private static bool IsPredicateExpression(SqlExpression exp)
        {
            switch (exp.NodeType)
            {
                case SqlNodeType.And:
                case SqlNodeType.Between:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.Exists:
                case SqlNodeType.In:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.LE:
                case SqlNodeType.Like:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.Or:
                    return true;

                case SqlNodeType.Lift:
                    return IsPredicateExpression(((SqlLift) exp).Expression);
            }
            return false;
        }

        internal override SqlStatement VisitAssign(SqlAssign sa)
        {
            sa.LValue = this.VisitExpression(sa.LValue);
            sa.RValue = this.VisitExpression(sa.RValue);
            return sa;
        }

        internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
        {
            if (bo.NodeType.IsBinaryOperatorExpectingPredicateOperands())
            {
                bo.Left = this.VisitPredicate(bo.Left);
                bo.Right = this.VisitPredicate(bo.Right);
                return bo;
            }
            bo.Left = this.VisitExpression(bo.Left);
            bo.Right = this.VisitExpression(bo.Right);
            return bo;
        }

        internal override SqlExpression VisitExpression(SqlExpression exp)
        {
            exp = (SqlExpression) base.Visit(exp);
            if ((exp != null) && IsPredicateExpression(exp))
            {
                exp = this.ConvertPredicateToValue(exp);
            }
            return exp;
        }

        internal override SqlSource VisitJoin(SqlJoin join)
        {
            join.Left = this.VisitSource(join.Left);
            join.Right = this.VisitSource(join.Right);
            join.Condition = this.VisitPredicate(join.Condition);
            return join;
        }

        internal override SqlExpression VisitLift(SqlLift lift)
        {
            lift.Expression = base.VisitExpression(lift.Expression);
            return lift;
        }

        internal SqlExpression VisitPredicate(SqlExpression exp)
        {
            exp = (SqlExpression) base.Visit(exp);
            if ((exp != null) && !IsPredicateExpression(exp))
            {
                exp = this.ConvertValueToPredicate(exp);
            }
            return exp;
        }

        internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
        {
            int num = 0;
            int count = c.Whens.Count;
            while (num < count)
            {
                SqlWhen when = c.Whens[num];
                when.Match = this.VisitPredicate(when.Match);
                when.Value = this.VisitExpression(when.Value);
                num++;
            }
            c.Else = this.VisitExpression(c.Else);
            return c;
        }

        internal override SqlSelect VisitSelect(SqlSelect select)
        {
            select.From = this.VisitSource(select.From);
            select.Where = this.VisitPredicate(select.Where);
            int num = 0;
            int count = select.GroupBy.Count;
            while (num < count)
            {
                select.GroupBy[num] = this.VisitExpression(select.GroupBy[num]);
                num++;
            }
            select.Having = this.VisitPredicate(select.Having);
            int num3 = 0;
            int num4 = select.OrderBy.Count;
            while (num3 < num4)
            {
                select.OrderBy[num3].Expression = this.VisitExpression(select.OrderBy[num3].Expression);
                num3++;
            }
            select.Top = this.VisitExpression(select.Top);
            select.Row = (SqlRow) this.Visit(select.Row);
            return select;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
        {
            if (uo.NodeType.IsUnaryOperatorExpectingPredicateOperand())
            {
                uo.Operand = this.VisitPredicate(uo.Operand);
                return uo;
            }
            uo.Operand = this.VisitExpression(uo.Operand);
            return uo;
        }
    }
}