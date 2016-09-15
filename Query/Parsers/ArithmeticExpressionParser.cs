using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ALinq.Dynamic.Parsers
{
    class ArithmeticExpressionParser : UnaryExpressionParser
    {
        //private IParser unaryParser;
        //private TokenCursor tokenCursor;
        public ArithmeticExpressionParser(TokenCursor tokenCursor)
            : this(tokenCursor, null)
        {

        }
        public ArithmeticExpressionParser(TokenCursor tokenCursor, IEnumerable<ObjectParameter> parameters)
            : base(tokenCursor, parameters)
        {
            //if (tokenCursor == null)
            //    throw Error.ArgumentNull("tokenCursor");

            //this.tokenCursor = tokenCursor;
        }


        //private Token Token
        //{
        //    get { return this.tokenCursor.Current; }
        //}

        //public TokenCursor TokenCursor
        //{
        //    get
        //    {
        //        return tokenCursor;
        //    }
        //    set
        //    {
        //        tokenCursor = value;
        //    }
        //}

        //void NextToken()
        //{
        //    this.tokenCursor.NextToken();
        //}

        private bool TokenIdentifierIs(string id)
        {
            return Token.Identity == TokenId.Identifier
                     && String.Equals(id, Token.Text, StringComparison.OrdinalIgnoreCase);
        }

        public override Expression ParseExpression()
        {
            return this.ParseAdditive();
        }

        // +, -, & operators
        private Expression ParseAdditive()
        {
            Expression left = ParseMultiplicative();
            while (Token.Identity == TokenId.Plus || Token.Identity == TokenId.Minus ||
                   Token.Identity == TokenId.Amphersand)
            {
                var op = Token;
                NextToken();
                Expression right = ParseMultiplicative();
                switch (op.Identity)
                {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                            goto case TokenId.Amphersand;

                        ExpressionUtility.CheckAndPromoteOperands(typeof(IAddSignatures), op.Text, ref left, ref right, op.Position);
                        left = GenerateAdd(left, right);
                        break;
                    case TokenId.Minus:
                        ExpressionUtility.CheckAndPromoteOperands(typeof(IAddSignatures), op.Text, ref left, ref right, op.Position);
                        left = GenerateSubtract(left, right);
                        break;
                    case TokenId.Amphersand:
                        left = GenerateStringConcat(left, right);
                        break;
                }
            }
            return left;
        }

        Expression ParseMultiplicative()
        {
            Expression left = ParseUnary();
            while (Token.Identity == TokenId.Asterisk || Token.Identity == TokenId.Slash ||
                   Token.Identity == TokenId.Percent || TokenIdentifierIs("mod"))
            {
                var op = Token;
                NextToken();
                Expression right = ParseUnary();
                ExpressionUtility.CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.Text, ref left, ref right, op.Position);
                if (op.Is(TokenId.Asterisk))
                {
                    left = Expression.Multiply(left, right);
                }
                else if (op.Is(TokenId.Slash))
                {
                    left = Expression.Divide(left, right);
                }
                else if (op.Is(TokenId.Percent) || op.Is(TokenId.Identifier))
                {
                    left = Expression.Modulo(left, right);
                }
            }
            return left;
        }

        private Expression ParseUnary()
        {
            if (Token.Identity == TokenId.Minus)
            {
                var op = Token;
                NextToken();

                Expression expr = ParseUnary();
                ExpressionUtility.CheckAndPromoteOperand(typeof(INegationSignatures), op.Text, ref expr, op.Position);
                expr = Expression.Negate(expr);
                return expr;
            }


            return base.ParseExpression();
        }


        private Expression GenerateAdd(Expression left, Expression right)
        {

            if (left.Type == typeof(string) && right.Type == typeof(string))
            {
                return ExpressionUtility.GenerateStaticMethodCall("Concat", left, right);
            }
            return Expression.Add(left, right);
        }

        private Expression GenerateSubtract(Expression left, Expression right)
        {
            return Expression.Subtract(left, right);
        }

        private Expression GenerateStringConcat(Expression left, Expression right)
        {
            return Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

    }
}
