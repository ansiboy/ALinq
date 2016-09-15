using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ALinq.Dynamic.Parsers
{
    class LogicalExpressionParser : ArithmeticExpressionParser
    {
        public LogicalExpressionParser(TokenCursor tokenCursor)
            : this(tokenCursor, null)
        {

        }

        public LogicalExpressionParser(TokenCursor tokenCursor, IEnumerable<ObjectParameter> parameters)
            : base(tokenCursor, parameters)
        {
        }

        public Expression ParseLogicalOr()
        {
            Expression left = ParseLogicalAnd();
            while (Token.Identity == TokenId.DoubleBar || Token.IdentifierIs("or"))
            {
                var op = Token;
                NextToken();
                Expression right = ParseLogicalAnd();
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        private Expression ParseLogicalAnd()
        {
            Expression left = ParseComparison();
            while (Token.Identity == TokenId.DoubleAmphersand || Token.IdentifierIs("and"))
            {
                var op = Token;
                NextToken();
                Expression right = ParseComparison();
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        private Expression ParseComparison()
        {
            Expression left = this.ParseAdditive();
            while (Token.Identity == TokenId.Equal || Token.Identity == TokenId.DoubleEqual ||
                   Token.Identity == TokenId.ExclamationEqual || Token.Identity == TokenId.LessGreater ||
                   Token.Identity == TokenId.GreaterThan || Token.Identity == TokenId.GreaterThanEqual ||
                   Token.Identity == TokenId.LessThan || Token.Identity == TokenId.LessThanEqual)
            {
                var op = Token;
                NextToken();
                Expression right = ParseAdditive();
                bool isEquality = op.Identity == TokenId.Equal || op.Identity == TokenId.DoubleEqual ||
                                  op.Identity == TokenId.ExclamationEqual || op.Identity == TokenId.LessGreater;
                if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType)
                {
                    if (left.Type != right.Type)
                    {
                        if (left.Type.IsAssignableFrom(right.Type))
                        {
                            right = Expression.Convert(right, left.Type);
                        }
                        else if (right.Type.IsAssignableFrom(left.Type))
                        {
                            left = Expression.Convert(left, right.Type);
                        }
                        else
                        {
                            throw Error.IncompatibleOperandsError(op.Text, left, right, op.Position);
                        }
                    }
                }
                else if (TypeUtility.IsEnumType(left.Type) || TypeUtility.IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        Expression e;
                        if ((e = ExpressionUtility.PromoteExpression(right, left.Type, true)) != null)
                        {
                            right = e;
                        }
                        else if ((e = ExpressionUtility.PromoteExpression(left, right.Type, true)) != null)
                        {
                            left = e;
                        }
                        else
                        {
                            throw Error.IncompatibleOperandsError(op.Text, left, right, op.Position);
                        }
                    }
                }
                else
                {
                    ExpressionUtility.CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                                            op.Text, ref left, ref right, op.Position);
                }

                switch (op.Identity)
                {
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = ExpressionUtility.GenerateEqual(left, right);
                        break;
                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = ExpressionUtility.GenerateNotEqual(left, right);
                        break;
                    case TokenId.GreaterThan:
                        left = ExpressionUtility.GenerateGreaterThan(left, right);
                        break;
                    case TokenId.GreaterThanEqual:
                        left = ExpressionUtility.GenerateGreaterThanEqual(left, right);
                        break;
                    case TokenId.LessThan:
                        left = ExpressionUtility.GenerateLessThan(left, right);
                        break;
                    case TokenId.LessThanEqual:
                        left = ExpressionUtility.GenerateLessThanEqual(left, right);
                        break;
                }
            }
            return left;
        }

        Expression ParseAdditive()
        {
            Expression expr;
            if (this.Token.Identity == TokenId.Exclamation || this.Token.IdentifierIs("not"))
                expr = ParseUnary();
            else
                expr = base.ParseExpression();

            return expr;
        }

        //!, not unary operators
        private Expression ParseUnary()
        {
            //Debug.Assert(Token.Identity == TokenId.Exclamation || TokenIdentifierIs("not"));
            Debug.Assert(Token.Identity != TokenId.Minus);
            if (Token.Identity == TokenId.Exclamation || Token.IdentifierIs("not"))
            {
                var op = Token;
                NextToken();

                Expression expr = ParseUnary();
                ExpressionUtility.CheckAndPromoteOperand(typeof(INotSignatures), op.Text, ref expr, op.Position);
                expr = Expression.Not(expr);

                return expr;
            }

            //return unaryParser.ParsePrimary();
            var e = base.ParseExpression();
            return e;
        }



        public override Expression ParseExpression()
        {
            return this.ParseLogicalOr();
        }
    }

}
