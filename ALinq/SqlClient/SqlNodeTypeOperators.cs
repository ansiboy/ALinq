namespace ALinq.SqlClient
{
    internal static class SqlNodeTypeOperators
    {
        // Methods
        internal static bool IsBinaryOperatorExpectingPredicateOperands(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.Add:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.Sub:
                    return false;

                case SqlNodeType.And:
                case SqlNodeType.Or:
                    return true;
            }
            throw Error.UnexpectedNode(nodeType);
        }

        internal static bool IsClientAidedExpression(this SqlExpression expr)
        {
            SqlNodeType nodeType = expr.NodeType;
            if (nodeType <= SqlNodeType.Link)
            {
                switch (nodeType)
                {
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.Element:
                    case SqlNodeType.Link:
                        goto Label_002C;
                }
                goto Label_002E;
            }
            if (((nodeType != SqlNodeType.Multiset) && (nodeType != SqlNodeType.New)) &&
                (nodeType != SqlNodeType.TypeCase))
            {
                goto Label_002E;
            }
            Label_002C:
            return true;
            Label_002E:
            return false;
        }

        internal static bool IsComparisonOperator(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.And:
                case SqlNodeType.Add:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.Or:
                case SqlNodeType.Sub:
                    return false;

                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                    return true;
            }
            throw Error.UnexpectedNode(nodeType);
        }

        internal static bool IsPredicateBinaryOperator(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.Add:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.Sub:
                    return false;

                case SqlNodeType.And:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.Or:
                    return true;
            }
            throw Error.UnexpectedNode(nodeType);
        }

        internal static bool IsPredicateUnaryOperator(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.Avg:
                case SqlNodeType.BitNot:
                case SqlNodeType.Cast:
                case SqlNodeType.LongCount:
                case SqlNodeType.Convert:
                case SqlNodeType.Count:
                case SqlNodeType.Covar:
                case SqlNodeType.ClrLength:
                case SqlNodeType.Negate:
                case SqlNodeType.Min:
                case SqlNodeType.Max:
                case SqlNodeType.Sum:
                case SqlNodeType.ValueOf:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.Stddev:
                    return false;

                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                    return true;
            }
            throw Error.UnexpectedNode(nodeType);
        }

        internal static bool IsUnaryOperatorExpectingPredicateOperand(this SqlNodeType nodeType)
        {
            SqlNodeType type = nodeType;
            if (type <= SqlNodeType.LongCount)
            {
                switch (type)
                {
                    case SqlNodeType.Avg:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.Cast:
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.LongCount:
                    case SqlNodeType.Convert:
                    case SqlNodeType.Count:
                    case SqlNodeType.Covar:
                    case SqlNodeType.ClrLength:
                        goto Label_00A9;
                }
                goto Label_00AB;
            }
            if (type <= SqlNodeType.Not2V)
            {
                switch (type)
                {
                    case SqlNodeType.Negate:
                    case SqlNodeType.Min:
                    case SqlNodeType.Max:
                        goto Label_00A9;

                    case SqlNodeType.New:
                        goto Label_00AB;

                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                        return true;
                }
                goto Label_00AB;
            }
            if (type <= SqlNodeType.Stddev)
            {
                switch (type)
                {
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.Stddev:
                        goto Label_00A9;
                }
                goto Label_00AB;
            }
            if ((type != SqlNodeType.Sum) && (type != SqlNodeType.ValueOf))
            {
                goto Label_00AB;
            }
            Label_00A9:
            return false;
            Label_00AB:
            throw Error.UnexpectedNode(nodeType);
        }
    }
}