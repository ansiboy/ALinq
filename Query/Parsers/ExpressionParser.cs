using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.Dynamic;
using ALinq.Dynamic.Parsers;

namespace ALinq.Dynamic.Parsers
{
    class ExpressionParser : UnaryExpressionParser
    {

        public ExpressionParser(TokenCursor tokenCursor)
            : this(tokenCursor, null)
        {

        }

        public ExpressionParser(TokenCursor tokenCursor, IEnumerable<ObjectParameter> parameters)
            : base(tokenCursor, parameters)
        {
        }


        public override Expression ParseExpression()
        {
            Expression expr = ParsePrefixKeyword();
            if (Token.Identity == TokenId.Identifier)
                expr = ParsePostKeyword(expr);

            return expr;
        }

        protected virtual IEnumerable<string> ReferenceNamespaces
        {
            get { return new string[0]; }
        }

        Expression ParsePrefixKeyword()
        {
            Expression expr;
            switch (Token.Keyword)
            {
                case Keyword.Not:
                    NextToken();
                    expr = ParseExpression();
                    expr = Expression.Not(expr);
                    break;
                case Keyword.Exists:
                    expr = ParseExists();
                    break;
                case Keyword.Case:
                    NextToken();
                    expr = ParseWhenThen();
                    break;
                case Keyword.Null:
                    NextToken();
                    expr = Literals.Null;
                    break;
                case Keyword.None:
                    expr = ParseLogicalOr();
                    break;
                default:
                    throw Error.KeywordNotSupported(Token, Token.Keyword.ToString());
            }
            return expr;
        }

        Expression ParsePostKeyword(Expression expr)
        {
            switch (Token.Keyword)
            {
                case Keyword.Between:
                    expr = ParseBetween(expr);
                    break;
                case Keyword.Not:
                    expr = ParseNot(expr);
                    break;
                case Keyword.Is:
                    expr = ParseIs(expr);
                    break;
                case Keyword.In:
                    expr = ParseIn(expr);
                    break;
                case Keyword.Like:
                    expr = ParseLike(expr);
                    break;
                case Keyword.Exists:
                    expr = this.ParseExists();
                    break;
                default:
                    break;
            }
            return expr;
        }

        //For Case 语句
        Expression ParseWhenThen()
        {
            if (Token.Keyword == Keyword.When)
            {
                NextToken();
                var errorToken = Token;
                var test = ParseExpression();
                if (Token.Keyword != Keyword.Then)
                    throw Error.ParseError(errorToken, () => Res.TokenExpected, Keyword.Then);

                var pos = Token.Position;
                NextToken();
                var then1 = ParseExpression();
                var then2 = ParseWhenThen();
                if (then2 == null)
                    then2 = then1.Type.IsValueType ? Expression.Constant(Convert.ChangeType(0, then1.Type), then1.Type) :
                                                     Expression.Constant(null, then1.Type);

                return ExpressionUtility.GenerateConditional(test, then1, then2, pos);

            }


            if (Token.Keyword == Keyword.Else)
            {
                NextToken();

                var errorToken = Token;
                var e = this.ParseExpression();
                if (Token.Keyword != Keyword.End)
                    throw Error.ParseError(errorToken, () => Res.TokenExpected, Keyword.End);

                NextToken();
                return e;
            }

            if (Token.Keyword == Keyword.End)
            {
                NextToken();
                return null;
            }

            throw Error.Token1OrToken2Expected(Token, Keyword.Then, Keyword.Else);
        }

        #region Class WhereFetch
        class WhereFetch : ExpressionVisitor
        {
            private Expression filter;

            public static Expression GetFilter(Expression expr)
            {
                var instance = new WhereFetch();
                instance.Visit(expr);
                return instance.filter;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Where")
                {
                    this.filter = node.Arguments[1];
                }
                return base.VisitMethodCall(node);
            }
        }
        #endregion

        private Expression ParseExists()
        {
            Debug.Assert(Token.Keyword == Keyword.Exists);


            NextToken();
            Token.Validate(TokenId.OpenParen, Res.OpenParenExpected);

            NextToken();

            var query = this.ParseExpression();
            //var expr = WhereFetch.GetFilter(query);

            //if (expr == null)
            //    throw Error.ParseError(Token, () => Res.TokenExpected, Keyword.Where);

            var any = ExpressionUtility.Call("Any", new[] { ExpressionUtility.ElementType(query) }, query);

            Token.Validate(TokenId.CloseParen, Res.CloseParenExpected);
            NextToken();

            return any;
        }

        private Expression ParseLike(Expression expr)
        {
            Debug.Assert(Token.Keyword == Keyword.Like);
            NextToken();

            var errorToken = Token;
            var op = this.ParseExpression();

            if (op.NodeType != ExpressionType.Constant)
                throw Error.LikeArgMustBeConstantOrParameter(errorToken);

            if (op.Type != typeof(string))
                throw Error.LikeArgMustBeStringType(errorToken);

            var str = (string)((ConstantExpression)op).Value;
            Debug.Assert(!string.IsNullOrEmpty(str));
            if (string.IsNullOrEmpty(str))
                throw Error.LikeArgMustBeNotEmpty(errorToken);

            str = str.Trim();

            var startsWithPercent = str[0] == '%';
            var endsWithPercent = str[str.Length - 1] == '%';
            MethodInfo m;
            if (startsWithPercent && !endsWithPercent)
            {
                m = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                str = str.Substring(1, str.Length - 1);
            }
            else if (!startsWithPercent && endsWithPercent)
            {
                m = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                str = str.Substring(0, str.Length - 1);
            }
            else if (startsWithPercent && endsWithPercent)
            {
                m = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                str = str.Substring(1, str.Length - 2);
            }
            else
                m = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            Debug.Assert(m != null);
            expr = Expression.Call(expr, m, Expression.Constant(str));
            return expr;
        }

        Expression ParseIn(Expression expr)
        {
            NextToken();
            var q = ParseExpression();
            if (typeof(IEnumerable).IsAssignableFrom(q.Type) == false)
                throw Error.InExpressionMustBeCollection(Token);

            var elementType = TypeUtility.GetElementType(q.Type);


            Debug.Assert(elementType != q.Type);

            var result = Expression.Call(typeof(Enumerable), "Contains", new[] { elementType }, q, expr);
            return result;
        }

        Expression ParseIs(Expression expr)
        {
            NextToken();
            bool isNot = false;
            if (Token.Keyword == Keyword.Not)
            {
                NextToken();
                isNot = true;
            }

            Expression e = null;
            switch (Token.Keyword)
            {
                case Keyword.Null:
                    {
                        NextToken();

                        var right = Expression.Constant(null);
                        e = isNot ? Expression.NotEqual(expr, right) : Expression.Equal(expr, right);
                        break;
                    }
                case Keyword.Of:
                    {
                        NextToken();

                        bool isOnly = false;
                        if (Token.Keyword == Keyword.Only)
                        {
                            isOnly = true;
                            NextToken();
                        }

                        if (string.IsNullOrEmpty(Token.Text))
                            throw Error.TypeNameExpected(Token);

                        var type = ParseType(); //this.ParseType(ReferenceNamespaces, true);
                        if (!isOnly)
                        {
                            e = Expression.TypeIs(expr, type);
                        }
                        else
                        {
                            var method = expr.Type.GetMethod("GetType");
                            var left = Expression.Call(expr, method);
                            var right = Expression.Constant(type);
                            e = Expression.MakeBinary(ExpressionType.Equal, left, right);
                        }
                        if (isNot)
                            e = Expression.Not(e);

                        NextToken();
                        break;
                    }

            }

            if (e == null)
                throw Error.Token1OrToken2Expected(Token, Keyword.Null, Keyword.Of);

            expr = e;
            return expr;
        }

        protected virtual Type ParseType()
        {
            return this.ParseType(new string[0], true);
        }


        Expression ParseNot(Expression expr)
        {
            NextToken();
            expr = ParsePostKeyword(expr);
            expr = Expression.Not(expr);
            return expr;
        }

        Expression ParseBetween(Expression expr)
        {
            NextToken();
            //var u = new UnaryExpressionParser(this.exprParser);
            //var parser = new UnaryExpressionParser(this.tokenCursor);

            var left = ParseUnary();
            ExpressionUtility.CheckAndPromoteOperands(typeof(IEqualitySignatures), ">=", ref expr, ref left, Token.Position);
            left = ExpressionUtility.GenerateGreaterThanEqual(expr, left);

            if (Token.Keyword != Keyword.And)
                throw Error.TokenExpected(Token, Keyword.And);

            NextToken();
            var right = ParseUnary();

            ExpressionUtility.CheckAndPromoteOperands(typeof(IEqualitySignatures), "<=", ref expr, ref right, Token.Position);
            right = ExpressionUtility.GenerateLessThanEqual(expr, right);

            expr = Expression.And(left, right);
            return expr;
        }

        // ||, or operator
        Expression ParseLogicalOr()
        {
            Expression left = ParseLogicalAnd();
            while (Token.Identity == TokenId.DoubleBar || Token.IdentifierIs("or"))
            {
                Token op = Token;
                NextToken();
                Expression right = ParseLogicalAnd();
                ExpressionUtility.CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Position);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        // &&, and operator
        Expression ParseLogicalAnd()
        {
            Expression left = ParseComparison();
            while (Token.Identity == TokenId.DoubleAmphersand || Token.IdentifierIs("and"))
            {
                Token op = Token;
                NextToken();
                Expression right = ParseComparison();
                ExpressionUtility.CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Position);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        Expression ParseComparison()
        {
            Expression left = ParseAdditive();
            while (Token.Identity == TokenId.Equal || Token.Identity == TokenId.DoubleEqual ||
                   Token.Identity == TokenId.ExclamationEqual || Token.Identity == TokenId.LessGreater ||
                   Token.Identity == TokenId.GreaterThan || Token.Identity == TokenId.GreaterThanEqual ||
                   Token.Identity == TokenId.LessThan || Token.Identity == TokenId.LessThanEqual)
            {
                Token op = Token;
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
                        if (right.Type == typeof(String) && right.NodeType == ExpressionType.Constant)
                        {
                            var rightValue = Enum.Parse(left.Type, (string)((ConstantExpression)right).Value);
                            right = Expression.Constant(rightValue);
                        }
                        else if (left.Type == typeof(String) && left.NodeType == ExpressionType.Constant)
                        {
                            var leftValue = Enum.Parse(right.Type, (string)((ConstantExpression)left).Value);
                            left = Expression.Constant(leftValue);
                        }

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

        // +, -, & operators
        Expression ParseAdditive()
        {
            Expression left = ParseMultiplicative();
            while (Token.Identity == TokenId.Plus || Token.Identity == TokenId.Minus ||
                   Token.Identity == TokenId.Amphersand)
            {
                Token op = Token;
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
                        ExpressionUtility.CheckAndPromoteOperands(typeof(ISubtractSignatures), op.Text, ref left, ref right, op.Position);
                        left = GenerateSubtract(left, right);
                        break;
                    case TokenId.Amphersand:
                        left = GenerateStringConcat(left, right);
                        break;
                }
            }
            return left;
        }

        // *, /, %, mod operators
        Expression ParseMultiplicative()
        {
            Expression left = ParseUnary();
            while (Token.Identity == TokenId.Asterisk || Token.Identity == TokenId.Slash ||
                   Token.Identity == TokenId.Percent || Token.IdentifierIs("mod"))
            {
                Token op = Token;
                NextToken();
                Expression right = ParseUnary();
                ExpressionUtility.CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.Text, ref left, ref right, op.Position);
                switch (op.Identity)
                {
                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);
                        break;
                    case TokenId.Slash:
                        left = Expression.Divide(left, right);
                        break;
                    case TokenId.Percent:
                    case TokenId.Identifier:
                        left = Expression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }

        // -, !, not unary operators
        protected Expression ParseUnary()
        {
            if (Token.Identity == TokenId.Minus || Token.Identity == TokenId.Exclamation ||
                Token.IdentifierIs("not"))
            {
                Token op = Token;
                NextToken();
                //if (op.Identity == TokenId.Minus && (Token.Identity == TokenId.IntegerLiteral ||
                //                               Token.Identity == TokenId.RealLiteral))
                //{
                //    Token.Text = "-" + Token.Text;
                //    Token.Position = op.Position;
                //    return ParsePrimary();
                //}
                Expression expr = ParseUnary();
                if (op.Identity == TokenId.Minus)
                {
                    ExpressionUtility.CheckAndPromoteOperand(typeof(INegationSignatures), op.Text, ref expr, op.Position);
                    expr = Expression.Negate(expr);
                }
                else
                {
                    ExpressionUtility.CheckAndPromoteOperand(typeof(INotSignatures), op.Text, ref expr, op.Position);
                    expr = Expression.Not(expr);
                }
                return expr;
            }
            return base.ParseExpression();
        }

        Expression GenerateAdd(Expression left, Expression right)
        {
            if (left.Type == typeof(string) && right.Type == typeof(string))
            {
                return GenerateStaticMethodCall("Concat", left, right);
            }
            return Expression.Add(left, right);
        }

        Expression GenerateSubtract(Expression left, Expression right)
        {
            return Expression.Subtract(left, right);
        }

        Expression GenerateStringConcat(Expression left, Expression right)
        {
            return Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

        MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
        {
            return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
        {
            return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
        }
    }
}