
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.Dynamic
{
    //TODO:添加 is of type
    //TODO:is [not] null
    //TODO:CAST 
    /// <summary>
    /// 用來解釋查詢語句中的表達式。
    /// </summary>
    internal sealed class ExpressionParser
    {
        #region predefinedTypes
        static readonly Type[] predefinedTypes = {
                                                     typeof(Object),
                                                     typeof(Boolean),
                                                     typeof(Char),
                                                     typeof(String),
                                                     typeof(SByte),
                                                     typeof(Byte),
                                                     typeof(Int16),
                                                     typeof(UInt16),
                                                     typeof(Int32),
                                                     typeof(UInt32),
                                                     typeof(Int64),
                                                     typeof(UInt64),
                                                     typeof(Single),
                                                     typeof(Double),
                                                     typeof(Decimal),
                                                     typeof(DateTime),
                                                     typeof(TimeSpan),
                                                     typeof(Guid),
                                                     typeof(Math),
                                                     typeof(Convert)
                                                 };
        #endregion

        #region Interface
        interface ILogicalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        interface IArithmeticSignatures
        {
            void F(int x, int y);
            void F(uint x, uint y);
            void F(long x, long y);
            void F(ulong x, ulong y);
            void F(float x, float y);
            void F(double x, double y);
            void F(decimal x, decimal y);
            void F(int? x, int? y);
            void F(uint? x, uint? y);
            void F(long? x, long? y);
            void F(ulong? x, ulong? y);
            void F(float? x, float? y);
            void F(double? x, double? y);
            void F(decimal? x, decimal? y);
        }

        interface IRelationalSignatures : IArithmeticSignatures
        {
            void F(string x, string y);
            void F(char x, char y);
            void F(DateTime x, DateTime y);
            void F(TimeSpan x, TimeSpan y);
            void F(char? x, char? y);
            void F(DateTime? x, DateTime? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        interface IEqualitySignatures : IRelationalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        interface IAddSignatures : IArithmeticSignatures
        {
            void F(DateTime x, TimeSpan y);
            void F(TimeSpan x, TimeSpan y);
            void F(DateTime? x, TimeSpan? y);
            void F(TimeSpan? x, TimeSpan? y);
        }

        interface ISubtractSignatures : IAddSignatures
        {
            void F(DateTime x, DateTime y);
            void F(DateTime? x, DateTime? y);
        }

        #region INegationSignatures
        interface INegationSignatures
        {
            void F(int x);
            void F(long x);
            void F(float x);
            void F(double x);
            void F(decimal x);
            void F(int? x);
            void F(long? x);
            void F(float? x);
            void F(double? x);
            void F(decimal? x);
        }
        #endregion

        #region INotSignatures
        interface INotSignatures
        {
            void F(bool x);
            void F(bool? x);
        }
        #endregion

        #region IEnumerableSignatures
        interface IEnumerableSignatures
        {
            void Where(bool predicate);
            void Any();
            void Any(bool predicate);
            void All(bool predicate);
            void Count();
            void Count(bool predicate);
            void Min(object selector);
            void Max(object selector);
            void Sum(int selector);
            void Sum(int? selector);
            void Sum(long selector);
            void Sum(long? selector);
            void Sum(float selector);
            void Sum(float? selector);
            void Sum(double selector);
            void Sum(double? selector);
            void Sum(decimal selector);
            void Sum(decimal? selector);
            void Average(int selector);
            void Average(int? selector);
            void Average(long selector);
            void Average(long? selector);
            void Average(float selector);
            void Average(float? selector);
            void Average(double selector);
            void Average(double? selector);
            void Average(decimal selector);
            void Average(decimal? selector);
        }
        #endregion
        #endregion

        #region Binary
        class Binary
        {
            private byte[] bytes;

            public Binary(byte[] bytes)
            {
                this.bytes = bytes;
            }

            //public static implicit operator byte[](Binary binary)
            //{
            //    if (binary != null)
            //        return binary.bytes;

            //    return null;
            //}

            public static explicit operator byte[](Binary binary)
            {
                if (binary != null)
                    return binary.bytes;

                return null;
            }
        }
        #endregion

        enum Keyword
        {
            None,

            And,
            Between,
            False,
            IIF,
            In,
            Is,
            Of,
            OfType,
            True,
            Value,
            MultiSet,
            New,
            Not,
            Now,
            Null,
            Row,

            As
        }

        enum Function
        {
            Cast,
        }

        static readonly Expression trueLiteral = Expression.Constant(true);
        static readonly Expression falseLiteral = Expression.Constant(false);
        static readonly Expression nullLiteral = Expression.Constant(null);

        //private const string keywordNew = "new";
        ////private const string keywordIt = "it";
        //private const string keywordIif = "iif";
        //private const string keywordRow = "row";
        //private const string keywordThis = "this";
        //private const string keywordValue = "value";
        //private const string keywordMultiSet = "multiset";
        //private const string keywordIn = "in";

        //class SqlMethods
        //{
        //    public static object Cast(object value, Type targetType)
        //    {
        //        return Convert.ChangeType(value, targetType);
        //    }
        //}

        private Dictionary<string, object> keywords;
        private Dictionary<string, object> functions;

        private Dictionary<string, object> symbols;
        Dictionary<Expression, string> literals;
        private ITokenCursor tokenCursor;
        private Func<string, object> getSymbol;
        private ParameterExpression it;


        private ExpressionParser()
        {
            this.symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this.literals = new Dictionary<Expression, string>();

            ProcessFunctions();
            ProcessKeywords();
        }

        public ExpressionParser(string expression, IEnumerable<ParameterExpression> variables, ObjectParameter[] parameters)
            : this()
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            this.tokenCursor = new TokenCursor(expression);

            if (variables != null)
                ProcessVariables(variables);

            if (parameters != null)
                ProcessParameters(parameters);

        }

        public ExpressionParser(ITokenCursor tokenCursor, Func<string, object> getSymbol)
            : this()
        {
            if (tokenCursor == null)
                throw new ArgumentNullException("tokenCursor");

            this.tokenCursor = tokenCursor;
            this.getSymbol = getSymbol;
        }

        internal IToken Token
        {
            get { return tokenCursor.Current; }
        }

        internal ITokenCursor TokenCursor
        {
            get { return this.tokenCursor; }
        }

        void ProcessKeywords()
        {
            var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var keywordValues = Enum.GetValues(typeof(Keyword));
            foreach (var value in keywordValues)
            {
                var name = Enum.GetName(typeof(Keyword), value);
                d.Add(name, value);
            }

            foreach (Type type in predefinedTypes)
                d.Add(type.Name, type);

            d.Add("Binary", typeof(byte[]));
            d.Add("X", typeof(byte[]));

            keywords = d;
        }

        void ProcessVariables(IEnumerable<ParameterExpression> parameters)
        {
            foreach (ParameterExpression pe in parameters)
                if (!String.IsNullOrEmpty(pe.Name))
                    AddSymbol(pe.Name, pe);

            this.it = parameters.SingleOrDefault(o => o.Name == string.Empty);
        }

        void ProcessParameters(IEnumerable<ObjectParameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                AddSymbol("@" + parameter.Name, parameter.Value);
            }
        }

        void AddSymbol(string name, object value)
        {
            if (symbols.ContainsKey(name))
                throw ParseError(Res.DuplicateIdentifier, name);
            symbols.Add(name, value);
        }

        private bool GetSymbol(string symbolName, out object value)
        {
            Debug.Assert(symbols != null);
            if (symbols.TryGetValue(symbolName, out value))
                return true;

            if (this.getSymbol != null)
            {
                value = this.getSymbol(symbolName);
                return value != null;
            }

            return false;
        }

        public Expression Parse()
        {
            return this.Parse(null);
        }

        public Expression Parse(Type resultType)
        {
            var exprPos = Token.Position;
            Expression expr = ParseExpression();

            if (string.Equals(Token.Text, Keyword.In.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                expr = ParseIn(expr);
            }

            if (resultType != null)
                if ((expr = PromoteExpression(expr, resultType, true)) == null)
                    throw ParseError(exprPos, Res.ExpressionTypeMismatch, GetTypeName(resultType));

            if (Token.Identity != TokenId.End)
                throw Error.GenericSyntaxError(Token.Position);

            return expr;
        }

        public Expression ParseSelection()
        {
            Expression expr;
            switch (Token.Text.ToLower())
            {
                case "value":
                    expr = ParseExpression();
                    break;
                case "row":
                default:
                    expr = ParseRow();
                    break;
                case "new":
                    expr = ParseNew();
                    break;
            }

            if (Token.Identity != TokenId.End)
                throw Error.GenericSyntaxError(Token.Position);

            return expr;
        }



        // ?: operator
        public Expression ParseExpression()
        {
            int errorPos = Token.Position;
            Expression expr = ParseLogicalOr();



            expr = this.ParsePostKeyword(expr);

            return expr;
        }

        /// <summary>
        /// 解释后置的关键字
        /// </summary>
        private Expression ParsePostKeyword(Expression expr)
        {
            object value;
            keywords.TryGetValue(Token.Text, out value);
            if (value is Keyword == false)
                return expr;

            switch ((Keyword)value)
            {
                case Keyword.Between:
                    {
                        NextToken();
                        var left = ParseAdditive();
                        left = Expression.GreaterThanOrEqual(expr, left);

                        Debug.Assert(this.TokenIdentifierIs(Keyword.And));//TODO:判断 Token 为 And

                        NextToken();
                        var right = ParseAdditive();
                        right = Expression.LessThanOrEqual(expr, right);

                        expr = Expression.And(left, right);
                        break;
                    }
                case Keyword.Not:
                    {
                        NextToken();
                        var op = ParsePostKeyword(expr);
                        expr = Expression.Not(op);
                        break;
                    }
                case Keyword.Is:
                    {
                        NextToken();

                        //object value1;
                        //keywords.TryGetValue(Token.Text, out value1);
                        //if (value1 is Keyword == false)
                        //    break;

                        //var keyword = (Keyword)value1;
                        var keyword = ParseTokenAsKeyword();
                        if (keyword == Keyword.None)
                            break;

                        bool isNot = false;
                        if (keyword == Keyword.Not)
                        {
                            NextToken();
                            isNot = true;
                        }

                        keyword = ParseTokenAsKeyword();
                        Expression e = null;
                        if (keyword == Keyword.Null)
                        {
                            NextToken();

                            var right = nullLiteral;
                            e = isNot ? Expression.NotEqual(expr, right) : Expression.Equal(expr, right);

                        }
                        else if (keyword == Keyword.Of)
                        {
                            NextToken();
                            var op = ParseExpression();

                            Debug.Assert(op.NodeType == ExpressionType.Constant);
                            Debug.Assert(op.Type == typeof(Type).GetType());

                            e = Expression.TypeIs(expr, (Type)((ConstantExpression)op).Value);
                            if (isNot)
                                e = Expression.Not(e);
                        }

                        if (e == null)
                            throw new Exception("Expected keyword NULL or OF.");//TODO:統一異常

                        expr = e;
                        break;
                    }
                case Keyword.Null:
                    {
                        expr = Expression.Equal(expr, nullLiteral);
                        break;
                    }
            }


            return expr;
        }

        Keyword ParseTokenAsKeyword()
        {
            object value;
            if (keywords.TryGetValue(Token.Text, out value))
            {
                if (value is Keyword)
                    return (Keyword)value;
            }

            return Keyword.None;
        }

        // ||, or operator
        Expression ParseLogicalOr()
        {
            Expression left = ParseLogicalAnd();
            while (Token.Identity == TokenId.DoubleBar || TokenIdentifierIs("or"))
            {
                var op = Token;
                NextToken();
                Expression right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Position);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        // &&, and operator
        Expression ParseLogicalAnd()
        {
            Expression left = ParseComparison();
            while (Token.Identity == TokenId.DoubleAmphersand || TokenIdentifierIs(Keyword.And))
            {
                var op = Token;
                NextToken();
                Expression right = ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Position);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        private Expression ParseComparison()
        {
            Expression left = ParseAdditive();
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
                            throw IncompatibleOperandsError(op.Text, left, right, op.Position);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        Expression e;
                        if ((e = PromoteExpression(right, left.Type, true)) != null)
                        {
                            right = e;
                        }
                        else if ((e = PromoteExpression(left, right.Type, true)) != null)
                        {
                            left = e;
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.Text, left, right, op.Position);
                        }
                    }
                }
                else
                {
                    #region MyRegion
                    ////Try to change type of the value to the target type.
                    //var converted = false;
                    //try
                    //{
                    //    if (IsDateTimeType(left.Type) && right.Type == typeof(string))
                    //    {
                    //        if (right.NodeType == ExpressionType.Constant)
                    //        {
                    //            var value = Convert.ToDateTime(((ConstantExpression)right).Value);
                    //            if (IsNullableType(left.Type))
                    //            {
                    //                right = Expression.Constant(value, typeof(DateTime?));
                    //            }
                    //            else
                    //            {
                    //                right = Expression.Constant(value);
                    //            }
                    //            converted = true;
                    //        }
                    //    }
                    //    if (IsDateTimeType(right.Type) && left.Type == typeof(string))
                    //    {
                    //        if (left.NodeType == ExpressionType.Constant)
                    //        {
                    //            var value = Convert.ToDateTime(((ConstantExpression)left).Value);
                    //            left = Expression.Constant(value);
                    //            converted = true;
                    //        }
                    //    }
                    //}
                    //finally
                    //{
                    //    if (!converted)
                    //CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                    //    op.text, ref left, ref right, op.pos);
                    //} 
                    #endregion
                    CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                                            op.Text, ref left, ref right, op.Position);
                }
                switch (op.Identity)
                {
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = GenerateEqual(left, right);
                        break;
                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = GenerateNotEqual(left, right);
                        break;
                    case TokenId.GreaterThan:
                        left = GenerateGreaterThan(left, right);
                        break;
                    case TokenId.GreaterThanEqual:
                        left = GenerateGreaterThanEqual(left, right);
                        break;
                    case TokenId.LessThan:
                        left = GenerateLessThan(left, right);
                        break;
                    case TokenId.LessThanEqual:
                        left = GenerateLessThanEqual(left, right);
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
                var op = Token;
                NextToken();
                Expression right = ParseMultiplicative();
                switch (op.Identity)
                {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                            goto case TokenId.Amphersand;
                        CheckAndPromoteOperands(typeof(IAddSignatures), op.Text, ref left, ref right, op.Position);
                        left = GenerateAdd(left, right);
                        break;
                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.Text, ref left, ref right, op.Position);
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
                   Token.Identity == TokenId.Percent || TokenIdentifierIs("mod"))
            {
                var op = Token;
                NextToken();
                Expression right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.Text, ref left, ref right, op.Position);
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

        // -, !, not unary operators
        private Expression ParseUnary()
        {
            if (Token.Identity == TokenId.Minus || Token.Identity == TokenId.Exclamation ||
                TokenIdentifierIs(Keyword.Not))
            {
                var op = Token;
                NextToken();

                Expression expr = ParseUnary();
                if (op.Identity == TokenId.Minus)
                {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.Text, ref expr, op.Position);
                    expr = Expression.Negate(expr);
                }
                else
                {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.Text, ref expr, op.Position);
                    expr = Expression.Not(expr);
                }
                return expr;
            }

            return ParsePrimary();
        }

        public Expression ParsePrimary()
        {
            Expression expr = ParsePrimaryStart();
            while (true)
            {
                if (Token.Identity == TokenId.Dot)
                {
                    NextToken();
                    expr = ParseMemberAccess(expr);
                }
                else if (Token.Identity == TokenId.OpenBracket)
                {
                    expr = ParseElementAccess(expr);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private Expression ParsePrimaryStart()
        {
            if (Token.IsIdentifier())
            {
                return ParseIdentifier();
            }
            if (Token.Identity == TokenId.StringLiteral)
            {
                return ParseStringLiteral();
            }
            if (Token.Identity == TokenId.Sharp)
            {
                return ParseDateTimeLiteral();
            }
            if (Token.Identity == TokenId.IntegerLiteral)
            {
                return ParseIntegerLiteral();
            }
            if (Token.Identity == TokenId.RealLiteral)
            {
                return ParseRealLiteral();
            }
            if (Token.Identity == TokenId.OpenParen)
            {
                return ParseParenExpression();
            }
            //if (Token.Identity == TokenId.OpenBracket)
            //{
            //    return ParseElementAccess(this.it);
            //}
            if (Token.Identity == TokenId.OpenCurlyBrace)
            {
                return ParseMultiSet();
            }
            //throw ParseError(Res.ExpressionExpected);
            throw Error.GenericSyntaxError(Token.Position);
        }

        private Expression ParseValue()
        {
            switch (Token.Identity)
            {
                case TokenId.StringLiteral:
                    return ParseStringLiteral();
                case TokenId.Sharp:
                    return ParseDateTimeLiteral();
                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return ParseRealLiteral();
                default:
                    throw ParseError(Res.ExpressionExpected);
            }
        }

        ConstantExpression ParseStringLiteral()
        {
            ValidateToken(TokenId.StringLiteral);
            char quote = Token.Text[0];
            string s = Token.Text.Substring(1, Token.Text.Length - 2);
            int start = 0;
            while (true)
            {
                int i = s.IndexOf(quote, start);
                if (i < 0) break;
                s = s.Remove(i, 1);
                start = i + 1;
            }
            NextToken();

            return CreateLiteral(s, s);
        }



        Expression ParseDateTimeLiteral()
        {
            ValidateToken(TokenId.Sharp);
            char quote = Token.Text[0];
            string s = Token.Text.Substring(1, Token.Text.Length - 2);
            int start = 0;
            while (true)
            {
                int i = s.IndexOf(quote, start);
                if (i < 0) break;
                s = s.Remove(i, 1);
                start = i + 1;
            }
            NextToken();
            //TODO:验证字符串格式
            return CreateLiteral(Convert.ToDateTime(s), s);
        }


        Expression ParseIntegerLiteral()
        {
            ValidateToken(TokenId.IntegerLiteral);
            string text = Token.Text;

            char last = text[text.Length - 1];
            char? lastPrevious = null;
            if (text.Length > 1)
                lastPrevious = text[text.Length - 2];

            if ((lastPrevious == 'U' || lastPrevious == 'u') && (last == 'L' || last == 'l'))
            {
                ulong value;
                if (!ulong.TryParse(text.Substring(0, text.Length - 2), out value))
                    throw ParseError(Res.InvalidIntegerLiteral, text);

                NextToken();
                return CreateLiteral(value, text);
            }

            if (last == 'U' || last == 'u')
            {
                uint value;
                if (!uint.TryParse(text.Substring(0, text.Length - 1), out value))
                    throw ParseError(Res.InvalidIntegerLiteral, text);

                NextToken();
                return CreateLiteral(value, text);
            }

            if (last == 'L' || last == 'l')
            {
                long value;
                if (!long.TryParse(text.Substring(0, text.Length - 1), out value))
                    throw ParseError(Res.InvalidIntegerLiteral, text);

                NextToken();
                return CreateLiteral(value, text);
            }

            int int32;
            if (!int.TryParse(text, out int32))
                throw ParseError(Res.InvalidIntegerLiteral, text);

            NextToken();
            return CreateLiteral(int32, text);



            //ulong value;
            //if (!UInt64.TryParse(text, out value))
            //    throw ParseError(Res.InvalidIntegerLiteral, text);

            //NextToken();
            //if (value <= (ulong)Int32.MaxValue) 
            //    return CreateLiteral((int)value, text);
            //if (value <= (ulong)UInt32.MaxValue) 
            //    return CreateLiteral((uint)value, text);
            //if (value <= (ulong)Int64.MaxValue) 
            //    return CreateLiteral((long)value, text);

            //return CreateLiteral(value, text);
        }

        Expression ParseRealLiteral()
        {
            ValidateToken(TokenId.RealLiteral);
            string text = Token.Text;
            object value = null;
            char last = text[text.Length - 1];
            if (last == 'F' || last == 'f')
            {
                float f;
                if (Single.TryParse(text.Substring(0, text.Length - 1), out f)) value = f;
            }
            else
            {
                double d;
                if (Double.TryParse(text, out d)) value = d;
            }
            if (value == null) throw ParseError(Res.InvalidRealLiteral, text);
            NextToken();
            return CreateLiteral(value, text);
        }

        ConstantExpression CreateLiteral(object value, string text)
        {
            ConstantExpression expr = Expression.Constant(value);
            literals.Add(expr, text);
            return expr;
        }

        //Expression CreateLiteral<T>(T value, string text)
        //{
        //    ConstantExpression expr = Expression.Constant(value);
        //    literals.Add(expr, text);
        //    return expr;
        //}

        Expression ParseParenExpression()
        {
            Debug.Assert(Token.Identity == TokenId.OpenParen);
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            Expression e = ParseExpression();
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
            NextToken();
            return e;
        }

        private Expression ParseIdentifier()
        {
            ValidateToken(TokenId.Identifier);



            object value;
            if (functions.TryGetValue(Token.Text, out value))
            {
                MethodInfo method = value as MethodInfo;
                if (method != null)
                    return ParseStaticMethod(method);

                if (value is Function)
                {
                    return ParseStaticMethod((Function)value);
                }

                return ParseStaticMemberAccess((Type)value);
            }

            if (keywords.TryGetValue(Token.Text, out value))
            {
                if (value is Type)
                    return ParseTypeAccess((Type)value);

                if (value is Keyword)
                {
                    switch ((Keyword)value)
                    {
                        case Keyword.IIF:
                            return ParseIif();

                        case Keyword.New:
                            return ParseNew();

                        case Keyword.Row:
                            return ParseRow();

                        case Keyword.MultiSet:
                            return ParseMultiSet();

                        case Keyword.Value:
                            NextToken();

                            var expr = this.ParseExpression();

                            ValidateToken(TokenId.End, Res.SyntaxError);

                            return expr;

                        case Keyword.True:
                            value = trueLiteral;
                            break;

                        case Keyword.False:
                            value = falseLiteral;
                            break;

                        case Keyword.Null:
                            value = nullLiteral;
                            break;
                    }
                }

                NextToken();
                return (Expression)value;
            }
            if (GetSymbol(Token.Text, out value))
            {
                var expr = value as Expression;
                if (expr == null)
                {
                    expr = Expression.Constant(value);
                }
                else
                {
                    //处理函数调用
                    var lambda = expr as LambdaExpression;
                    if (lambda != null)
                        return ParseLambdaInvocation(lambda);
                }
                NextToken();
                return expr;
            }

            if (Token.Text[0] == '@')
                throw Error.ParameterWasNotDefined(Token.Text.Substring(1), Token.Position);

            if (it != null)
                return ParseMemberAccess(it);


            //throw ParseError(Res.UnknownIdentifier, token.Text);
            throw Error.UnknownIdentifier(Token.Text, Token.Position);
        }

        private Expression ParseStaticMethod(Function function)
        {
            MethodInfo method = null;
            if (function == Function.Cast)
            {
                NextToken();
                ValidateToken(TokenId.OpenParen);

                NextToken();

                var expr1 = ParseExpression();

                ValidateKeyword(Keyword.As);
                NextToken();

                Type type;
                object value;
                if (!keywords.TryGetValue(Token.Text, out value) || (type = value as Type) == null)
                {
                    throw Error.GenericSyntaxError(Token.Position);//TODO:显示异常信息。
                }

                var expr2 = Expression.Constant(type);
                var typeCode = Type.GetTypeCode(type);
                var methodName = "To" + typeCode;
                method = typeof(Convert).GetMethod(methodName, new[] { expr1.Type });

                NextToken();
                ValidateToken(TokenId.CloseParen);

                NextToken();

                var expr = Expression.Call(null, method, expr1);
                return expr;
            }

            //if (method != null)
            //    return ParseStaticMethod(method);

            throw new NotImplementedException(function.ToString());
        }




        private Expression ParseIif()
        {
            var errorPos = Token.Position;
            NextToken();
            Expression[] args = ParseArgumentList();
            if (args.Length != 3)
                throw ParseError(errorPos, Res.IifRequiresThreeArgs);
            return GenerateConditional(args[0], args[1], args[2], errorPos);
        }

        internal Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
        {
            if (test.Type != typeof(bool))
                throw ParseError(errorPos, Res.FirstExprMustBeBool);
            if (expr1.Type != expr2.Type)
            {
                Expression expr1as2 = expr2 != nullLiteral ? PromoteExpression(expr1, expr2.Type, true) : null;
                Expression expr2as1 = expr1 != nullLiteral ? PromoteExpression(expr2, expr1.Type, true) : null;
                if (expr1as2 != null && expr2as1 == null)
                {
                    expr1 = expr1as2;
                }
                else if (expr2as1 != null && expr1as2 == null)
                {
                    expr2 = expr2as1;
                }
                else
                {
                    string type1 = expr1 != nullLiteral ? expr1.Type.Name : "null";
                    string type2 = expr2 != nullLiteral ? expr2.Type.Name : "null";
                    if (expr1as2 != null && expr2as1 != null)
                        throw ParseError(errorPos, Res.BothTypesConvertToOther, type1, type2);
                    throw ParseError(errorPos, Res.NeitherTypeConvertsToOther, type1, type2);
                }
            }
            return Expression.Condition(test, expr1, expr2);
        }

        private Expression ParseNew()
        {
            NextToken();

            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();

            var expr = ParseObject(typeof(DynamicObject));

            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();
            //ValidateToken(TokenId.End, Res.CloseParenOrCommaExpected);

            //NextToken();
            return expr;
        }

        private Expression ParseRow()
        {
            Expression expr;

            if (Token.IsIdentifier() == false)
                throw ParseError(Res.TokenExpected);

            if (this.TokenIdentifierIs(Keyword.Row.ToString()))
            {
                this.ValidateToken(TokenId.Identifier, Keyword.Row.ToString());

                NextToken();

                ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
                NextToken();

                expr = ParseObject(typeof(DynamicRow));

                ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
                NextToken();

                NextToken();
            }
            else
            {
                expr = ParseObject(typeof(DynamicRow));
                NextToken();
            }

            return expr;
        }

        //1、語句：Select e.FirstName, e.LastName From Employees as e
        //將 e.FirstName, e.LastName 解釋成為表達式
        //2、語句：Select e From Employees as e
        //將 e 解釋成表達式
        private Expression ParseObject(Type newObjectBaseType)
        {
            #region OldCode
            //NextToken();
            //ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            //NextToken();
            //List<DynamicProperty> properties = new List<DynamicProperty>();
            //List<Expression> expressions = new List<Expression>();
            //while (true)
            //{
            //    int exprPos = token.pos;
            //    Expression expr = ParseExpression();
            //    string propName;
            //    if (TokenIdentifierIs("as"))
            //    {
            //        NextToken();
            //        propName = GetIdentifier();
            //        NextToken();
            //    }
            //    else
            //    {
            //        MemberExpression me = expr as MemberExpression;
            //        if (me == null) throw ParseError(exprPos, Res.MissingAsClause);
            //        propName = me.Member.Name;
            //    }
            //    expressions.Add(expr);
            //    properties.Add(new DynamicProperty(propName, expr.Type));
            //    if (token.id != TokenId.Comma) break;
            //    NextToken();
            //}
            //ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            //NextToken();
            //Type type = DynamicExpression.CreateClass(properties);
            //MemberBinding[] bindings = new MemberBinding[properties.Count];
            //for (int i = 0; i < bindings.Length; i++)
            //    bindings[i] = Expression.Bind(type.GetProperty(properties[i].Name), expressions[i]);
            //return Expression.MemberInit(Expression.New(type), bindings); 
            #endregion

            List<DynamicProperty> properties = new List<DynamicProperty>();
            List<Expression> expressions = new List<Expression>();

            while (true)
            {
                int exprPos = Token.Position;

                var expr = ParseExpression();


                string propName = null;
                if (TokenIdentifierIs("as"))
                {
                    NextToken();
                    propName = GetIdentifier();
                    NextToken();
                }
                else
                {
                    switch (expr.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            propName = ((MemberExpression)expr).Member.Name;
                            break;
                        case ExpressionType.Call:
                            {
                                var call = ((MethodCallExpression)expr);
                                if (call.Method.Name == "get_Item" && call.Arguments.Count == 1 &&
                                    call.Arguments[0].NodeType == ExpressionType.Constant)
                                {
                                    propName = ((ConstantExpression)call.Arguments[0]).Value.ToString();
                                }
                            }
                            break;
                        case ExpressionType.Parameter:
                            propName = ((ParameterExpression)expr).Name;
                            break;
                    }


                    if (propName == null)
                        throw ParseError(exprPos, Res.MissingAsClause);
                }
                expressions.Add(expr);
                properties.Add(new DynamicProperty(propName, expr.Type));
                if (Token.Identity != TokenId.Comma)
                    break;

                NextToken();
            }



            Type objectType = DynamicExpression.CreateClass(properties, newObjectBaseType);
            MemberBinding[] bindings = new MemberBinding[properties.Count];
            for (int i = 0; i < bindings.Length; i++)
                bindings[i] = Expression.Bind(objectType.GetProperty(properties[i].Name), expressions[i]);
            return Expression.MemberInit(Expression.New(objectType), bindings);
        }

        Expression ParseLambdaInvocation(LambdaExpression lambda)
        {
            int errorPos = Token.Position;
            NextToken();
            Expression[] args = ParseArgumentList();
            MethodBase method;
            if (FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
                throw ParseError(errorPos, Res.ArgsIncompatibleWithLambda);
            return Expression.Invoke(lambda, args);
        }

        private Expression ParseTypeAccess(Type type)
        {
            int errorPos = Token.Position;
            NextToken();

            if (Token.Identity == TokenId.Question)
            {
                if (!type.IsValueType || IsNullableType(type))
                    throw ParseError(errorPos, Res.TypeHasNoNullableForm, GetTypeName(type));
                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }

            if (Token.Identity == TokenId.StringLiteral)
            {
                var expr = ParseStringLiteral();
                //expr = Expression.Call(typeof(Convert).GetMethod("ToDateTime", new[] { expr.Type }), expr);
                //return expr;
                return this.ParseValueByType(type, expr);
            }

            if (Token.Identity == TokenId.OpenParen)
            {
                Expression[] args = ParseArgumentList();
                MethodBase method;
                switch (FindBestMethod(type.GetConstructors(), args, out method))
                {
                    case 0:
                        if (args.Length == 1)
                            return GenerateConversion(args[0], type, errorPos);
                        throw ParseError(errorPos, Res.NoMatchingConstructor, GetTypeName(type));
                    case 1:
                        return Expression.New((ConstructorInfo)method, args);
                    default:
                        throw ParseError(errorPos, Res.AmbiguousConstructorInvocation, GetTypeName(type));
                }
            }

            /*
            if (Token.Identity == TokenId.OpenBracket)
            {
                NextToken();
                var items = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type)); //new List<string>();
                if (Token.Identity != TokenId.CloseBracket)
                {
                    while (true)
                    {
                        var s = (ConstantExpression)ParseValue();

                        items.Add(s.Value);
                        if (Token.Identity != TokenId.Comma) break;
                        NextToken();
                    }
                }
                ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
                NextToken();
                return Expression.Constant(items);

            }
            */


            if (Token.Identity == TokenId.CloseParen)
            {
                NextToken();
                var expr = ParseUnary();
                expr = Expression.Convert(expr, type);
                return expr;
            }
            ValidateToken(TokenId.Dot, Res.OpenParenExpected);
            NextToken();

            return ParseStaticMemberAccess(type);
        }

        private Expression ParseValueByType(Type type, ConstantExpression expr)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DateTime:
                    var result = Expression.Call(typeof(Convert).GetMethod("ToDateTime", new[] { expr.Type }), expr);
                    return result;
                case TypeCode.String:
                    return expr;
                case TypeCode.Object:
                    if (type == typeof(Guid))
                    {
                        var constructor = typeof(Guid).GetConstructor(new[] { typeof(string) });
                        return Expression.New(constructor, expr);
                    }

                    if (type == typeof(byte[]))
                    {
                        var value = (String)expr.Value;
                        var bytes = StrToByte(value);
                        return Expression.Constant(bytes);
                    }

                    break;

            }
            throw Error.TypeNotSupported(type);
        }

        static byte[] StrToByte(string strValue)
        {
            int length = strValue.Length;
            byte[] buffer = new byte[length / 2];
            if ((length % 2) == 0)
            {
                for (int i = 0; i < (length / 2); i++)
                {
                    buffer[i] = Convert.ToByte(strValue.Substring(i * 2, 2), 0x10);
                }
            }
            return buffer;
        }




        Expression GenerateConversion(Expression expr, Type type, int errorPos)
        {
            Type exprType = expr.Type;
            if (exprType == type) return expr;
            if (exprType.IsValueType && type.IsValueType)
            {
                if ((IsNullableType(exprType) || IsNullableType(type)) &&
                    GetNonNullableType(exprType) == GetNonNullableType(type))
                    return Expression.Convert(expr, type);
                if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
                    (IsNumericType(type)) || IsEnumType(type))
                    return Expression.ConvertChecked(expr, type);
            }
            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
                exprType.IsInterface || type.IsInterface)
                return Expression.Convert(expr, type);
            throw ParseError(errorPos, Res.CannotConvertValue,
                             GetTypeName(exprType), GetTypeName(type));
        }


        Expression ParseStaticMethod(MethodInfo method)
        {
            Debug.Assert(method.ReturnType != typeof(void));

            NextToken();
            if (Token.Identity != TokenId.OpenParen)
                throw ParseError(Res.TokenExpected, TokenId.OpenParen);

            Expression[] args = ParseArgumentList();


            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
                throw ParseError(Res.NoMatchingMethod, method.Name);

            return Expression.Call(null, method, args);
        }

        Expression ParseStaticMemberAccess(Type type)
        {
            //instance = null;
            //if (instance != null) 
            //    type = instance.Type;

            int errorPos = Token.Position;
            string id = GetIdentifier();
            NextToken();
            if (Token.Identity != TokenId.OpenParen)
            {
                throw ParseError(Res.TokenExpected, TokenId.OpenParen);
            }

            Expression[] args = ParseArgumentList();
            MethodBase mb;

            var methodsCount = FindMethod(type, id, true, args, out mb);
            switch (methodsCount)
            {
                case 0:
                    throw ParseError(errorPos, Res.NoApplicableMethod, id, GetTypeName(type));
                case 1:
                    MethodInfo method = (MethodInfo)mb;
                    /*
                    if (!IsPredefinedType(method.DeclaringType))
                        throw ParseError(errorPos, Res.MethodsAreInaccessible, GetTypeName(method.DeclaringType));
                    */

                    if (method.ReturnType == typeof(void))
                        throw ParseError(errorPos, Res.MethodIsVoid, id, GetTypeName(method.DeclaringType));



                    return Expression.Call(null, (MethodInfo)method, args);
                default:
                    throw ParseError(errorPos, Res.AmbiguousMethodInvocation,
                                     id, GetTypeName(type));
            }


        }

        private Expression ParseMemberAccess(Expression instance)
        {
            //if (instance != null)
            var type = instance.Type;

            int errorPos = Token.Position;
            string id = GetIdentifier();
            NextToken();
            if (Token.Identity == TokenId.OpenParen)
            {
                return ParseMethodAccess(type, instance, id);
            }

            MemberInfo member = FindPropertyOrField(type, id, instance == null);
            if (member == null)
                throw ParseError(errorPos, Res.UnknownPropertyOrField, id, GetTypeName(type));

            Debug.Assert(instance != null);
            return Expression.MakeMemberAccess(instance, member);
        }


        internal Expression ParseMethodAccess(Type type, Expression instance, string methodName)
        {
            Expression[] args = ParseArgumentList();
            MethodBase mb;

            var errorPos = Token.Position;
            var methodsCount = FindMethod(type, methodName, instance == null, args, out mb);
            switch (methodsCount)
            {
                case 0:
                    throw ParseError(errorPos, Res.NoApplicableMethod, methodName, GetTypeName(type));
                case 1:
                    MethodInfo method = (MethodInfo)mb;
                    /*
                    if (!IsPredefinedType(method.DeclaringType))
                        throw ParseError(errorPos, Res.MethodsAreInaccessible, GetTypeName(method.DeclaringType));
                    */

                    if (method.ReturnType == typeof(void))
                        throw ParseError(errorPos, Res.MethodIsVoid, methodName, GetTypeName(method.DeclaringType));



                    return Expression.Call(instance, (MethodInfo)method, args);
                default:
                    throw ParseError(errorPos, Res.AmbiguousMethodInvocation, methodName, GetTypeName(type));
            }
        }

        Expression[] ParseArgumentList()
        {
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            Expression[] args = Token.Identity != TokenId.CloseParen ? ParseArguments() : new Expression[0];
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();
            return args;
        }

        Expression[] ParseArguments()
        {
            List<Expression> argList = new List<Expression>();
            while (true)
            {
                argList.Add(ParseExpression());
                if (Token.Identity != TokenId.Comma) break;
                NextToken();
            }
            return argList.ToArray();
        }

        private Expression ParseElementAccess(Expression expr)
        {
            int errorPos = Token.Position;
            ValidateToken(TokenId.OpenBracket, Res.OpenParenExpected);
            NextToken();
            Expression[] args;
            if (this.Token.Is(TokenId.Identifier))
            {
                args = new[] { Expression.Constant(Token.Text) };
                NextToken();
            }
            else
                args = ParseArguments();

            ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
            NextToken();
            if (expr.Type.IsArray)
            {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                    throw ParseError(errorPos, Res.CannotIndexMultiDimArray);
                Expression index = PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                    throw ParseError(errorPos, Res.InvalidIndex);
                return Expression.ArrayIndex(expr, index);
            }
            else
            {
                MethodBase mb;
                switch (FindIndexer(expr.Type, args, out mb))
                {
                    case 0:
                        throw ParseError(errorPos, Res.NoApplicableIndexer,
                                         GetTypeName(expr.Type));
                    case 1:
                        return Expression.Call(expr, (MethodInfo)mb, args);
                    default:
                        throw ParseError(errorPos, Res.AmbiguousIndexerInvocation,
                                         GetTypeName(expr.Type));
                }
            }
        }

        static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static Type GetNonNullableType(Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        static string GetTypeName(Type type)
        {
            Type baseType = GetNonNullableType(type);
            string s = baseType.Name;
            if (type != baseType) s += '?';
            return s;
        }

        static bool IsNumericType(Type type)
        {
            return GetNumericTypeKind(type) != 0;
        }

        static bool IsSignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 2;
        }

        static bool IsUnsignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 3;
        }

        static int GetNumericTypeKind(Type type)
        {
            type = GetNonNullableType(type);
            if (type.IsEnum) return 0;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return 1;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return 2;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 3;
                default:
                    return 0;
            }
        }

        static bool IsEnumType(Type type)
        {
            return GetNonNullableType(type).IsEnum;
        }

        void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
        {
            Expression[] args = new Expression[] { expr };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw ParseError(errorPos, Res.IncompatibleOperand,
                                 opName, GetTypeName(args[0].Type));
            expr = args[0];
        }

        void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
        {
            Expression[] args = new Expression[] { left, right };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw IncompatibleOperandsError(opName, left, right, errorPos);
            left = args[0];
            right = args[1];
        }

        Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
        {
            return ParseError(pos, Res.IncompatibleOperands,
                              opName, GetTypeName(left.Type), GetTypeName(right.Type));
        }

        private MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                                 (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
                                                     flags, Type.FilterNameIgnoreCase, memberName);
                if (members.Length != 0) return members[0];
            }
            return null;
        }

        int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                                 (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.FindMembers(MemberTypes.Method,
                                                     flags, Type.FilterNameIgnoreCase, methodName);

                int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count != 0)
                    return count;
            }

            method = null;
            return 0;
        }

        int FindIndexer(Type type, Expression[] args, out MethodBase method)
        {
            foreach (Type t in SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.GetMembers();//t.GetDefaultMembers();
                if (members.Length != 0)
                {
                    IEnumerable<MethodBase> methods = members.
                        OfType<PropertyInfo>().
                        Select(p => (MethodBase)p.GetGetMethod()).
                        Where(m => m != null);
                    int count = FindBestMethod(methods, args, out method);
                    if (count != 0) return count;
                }
            }
            method = null;
            return 0;
        }

        static IEnumerable<Type> SelfAndBaseTypes(Type type)
        {
            if (type.IsInterface)
            {
                List<Type> types = new List<Type>();
                AddInterface(types, type);
                return types;
            }
            return SelfAndBaseClasses(type);
        }

        static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        static void AddInterface(List<Type> types, Type type)
        {
            if (!types.Contains(type))
            {
                types.Add(type);
                foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
            }
        }

        class MethodData
        {
            public MethodBase MethodBase;
            public ParameterInfo[] Parameters;
            public Expression[] Args;
        }

        int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
        {
            MethodData[] applicable = methods.
                Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() }).
                Where(m => IsApplicable(m, args)).
                ToArray();
            if (applicable.Length > 1)
            {
                applicable = applicable.
                    Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).
                    ToArray();
            }
            if (applicable.Length == 1)
            {
                MethodData md = applicable[0];
                for (int i = 0; i < args.Length; i++) args[i] = md.Args[i];
                method = md.MethodBase;
            }
            else
            {
                method = null;
            }
            return applicable.Length;
        }

        bool IsApplicable(MethodData method, Expression[] args)
        {
            if (method.Parameters.Length != args.Length) return false;
            Expression[] promotedArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                ParameterInfo pi = method.Parameters[i];
                if (pi.IsOut) return false;
                Expression promoted = PromoteExpression(args[i], pi.ParameterType, false);
                if (promoted == null) return false;
                promotedArgs[i] = promoted;
            }
            method.Args = promotedArgs;
            return true;
        }

        Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            if (expr.Type == type) return expr;
            if (expr is ConstantExpression)
            {
                ConstantExpression ce = (ConstantExpression)expr;
                if (ce == nullLiteral)
                {
                    if (!type.IsValueType || IsNullableType(type))
                        return Expression.Constant(null, type);
                }
                else
                {
                    string text;
                    if (literals.TryGetValue(ce, out text))
                    {
                        Type target = GetNonNullableType(type);
                        Object value = null;
                        switch (Type.GetTypeCode(ce.Type))
                        {
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                value = ParseNumber(text, target);
                                break;
                            case TypeCode.Double:
                                if (target == typeof(decimal)) value = ParseNumber(text, target);
                                break;
                            case TypeCode.String:
                                value = ParseEnum(text, target);
                                break;
                        }
                        if (value != null)
                            return Expression.Constant(value, type);
                    }
                }
            }
            if (IsCompatibleWith(expr.Type, type))
            {
                if (type.IsValueType || exact) return Expression.Convert(expr, type);
                return expr;
            }
            return null;
        }

        static object ParseNumber(string text, Type type)
        {
            switch (Type.GetTypeCode(GetNonNullableType(type)))
            {
                case TypeCode.SByte:
                    sbyte sb;
                    if (sbyte.TryParse(text, out sb)) return sb;
                    break;
                case TypeCode.Byte:
                    byte b;
                    if (byte.TryParse(text, out b)) return b;
                    break;
                case TypeCode.Int16:
                    short s;
                    if (short.TryParse(text, out s)) return s;
                    break;
                case TypeCode.UInt16:
                    ushort us;
                    if (ushort.TryParse(text, out us)) return us;
                    break;
                case TypeCode.Int32:
                    int i;
                    if (int.TryParse(text, out i)) return i;
                    break;
                case TypeCode.UInt32:
                    uint ui;
                    if (uint.TryParse(text, out ui)) return ui;
                    break;
                case TypeCode.Int64:
                    long l;
                    if (long.TryParse(text, out l)) return l;
                    break;
                case TypeCode.UInt64:
                    ulong ul;
                    if (ulong.TryParse(text, out ul)) return ul;
                    break;
                case TypeCode.Single:
                    float f;
                    if (float.TryParse(text, out f)) return f;
                    break;
                case TypeCode.Double:
                    double d;
                    if (double.TryParse(text, out d)) return d;
                    break;
                case TypeCode.Decimal:
                    decimal e;
                    if (decimal.TryParse(text, out e)) return e;
                    break;
            }
            return null;
        }

        static object ParseEnum(string name, Type type)
        {
            if (type.IsEnum)
            {
                MemberInfo[] memberInfos = type.FindMembers(MemberTypes.Field,
                                                            BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static,
                                                            Type.FilterNameIgnoreCase, name);

                if (memberInfos.Length != 0)
                    return ((FieldInfo)memberInfos[0]).GetValue(null);
            }
            return null;
        }

        static bool IsCompatibleWith(Type source, Type target)
        {
            if (source == target) return true;
            if (!target.IsValueType) return target.IsAssignableFrom(source);
            Type st = GetNonNullableType(source);
            Type tt = GetNonNullableType(target);
            if (st != source && tt == target) return false;
            TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
            switch (sc)
            {
                case TypeCode.SByte:
                    switch (tc)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Byte:
                    switch (tc)
                    {
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
                            return true;
                    }
                    break;
                case TypeCode.Int16:
                    switch (tc)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (tc)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int32:
                    switch (tc)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (tc)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int64:
                    switch (tc)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (tc)
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Single:
                    switch (tc)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }
                    break;
                default:
                    if (st == tt) return true;
                    break;
            }
            return false;
        }

        static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
        {
            bool better = false;
            for (int i = 0; i < args.Length; i++)
            {
                int c = CompareConversions(args[i].Type,
                                           m1.Parameters[i].ParameterType,
                                           m2.Parameters[i].ParameterType);
                if (c < 0) return false;
                if (c > 0) better = true;
            }
            return better;
        }

        // Return 1 if s -> t1 is a better conversion than s -> t2
        // Return -1 if s -> t2 is a better conversion than s -> t1
        // Return 0 if neither conversion is better
        static int CompareConversions(Type s, Type t1, Type t2)
        {
            if (t1 == t2) return 0;
            if (s == t1) return 1;
            if (s == t2) return -1;
            bool t1t2 = IsCompatibleWith(t1, t2);
            bool t2t1 = IsCompatibleWith(t2, t1);
            if (t1t2 && !t2t1) return 1;
            if (t2t1 && !t1t2) return -1;
            if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2)) return 1;
            if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1)) return -1;
            return 0;
        }

        Expression GenerateEqual(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }

        Expression GenerateNotEqual(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }

        Expression GenerateGreaterThan(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.GreaterThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.GreaterThan(left, right);
        }

        Expression GenerateGreaterThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.GreaterThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.GreaterThanOrEqual(left, right);
        }

        Expression GenerateLessThan(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.LessThan(left, right);
        }

        Expression GenerateLessThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.LessThanOrEqual(left, right);
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

        void NextToken()
        {
            this.tokenCursor.NextToken();

        }



        private bool TokenIdentifierIs(string id)
        {
            return Token.Identity == TokenId.Identifier
                     && String.Equals(id, Token.Text, StringComparison.OrdinalIgnoreCase);
        }

        private bool TokenIdentifierIs(Keyword keyword)
        {
            return TokenIdentifierIs(keyword.ToString());
        }

        string GetIdentifier()
        {
            ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
            string id = Token.Text;
            if (id.Length > 1 && id[0] == '@') id = id.Substring(1);
            return id;
        }

        //void ValidateDigit()
        //{
        //    if (!Char.IsDigit(ch)) throw ParseError(textPos, Res.DigitExpected);
        //}

        private void ValidateKeyword(Keyword keyword)
        {
            var k = ParseTokenAsKeyword();
            if (k != keyword)
                throw ParseError(Res.SyntaxError);
        }

        private void ValidateToken(TokenId t, string errorMessage)
        {
            if (!Token.Is(t))
                throw ParseError(errorMessage);
        }

        private void ValidateToken(TokenId t)
        {
            if (!Token.Is(t))
                throw ParseError(Res.SyntaxError);
        }

        private Exception ParseError(string format, params object[] args)
        {
            return ParseError(Token.Position, format, args);
        }

        Exception ParseError(TokenPosition pos, string format, params object[] args)
        {
            return ParseError(pos.Sequence, format, args);
        }

        Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
        }

        void ProcessFunctions()
        {
            //if (functions == null)
            //{
            functions = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod;
            var concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            functions.Add(concat.Name, concat);

            //var abs = typeof(Math).GetMethod("Abs");
            functions.Add("ABS", typeof(Math));
            functions.Add("ACOS", typeof(Math));
            functions.Add("ASIN", typeof(Math));
            functions.Add("ATAN", typeof(Math));
            functions.Add("ATN2", typeof(Math));
            functions.Add("CEILING", typeof(Math));
            functions.Add("CAST", Function.Cast);
            functions.Add("COS", typeof(Math));
            functions.Add("COT", typeof(Math));
            //functions.Add("DEGREES"); //TODO: 沒有對應的函數
            functions.Add("EXP", typeof(Math));
            functions.Add("FLOOR", typeof(Math));
            functions.Add("LOG", typeof(Math));
            functions.Add("LOG10", typeof(Math));
            functions.Add("PI", typeof(Math));
            functions.Add("POWER", new KeyValuePair<string, object>("POW", typeof(Math)));
            //RADIANS //TODO: 沒有對應的函數
            //functions.Add("RAND", typeof (Math));//TODO: 沒有對應的函數

            //}

            //return;
        }

        private Expression ParseIn(Expression expr)
        {
            NextToken();
            var multi = this.ParseExpression();
            if (typeof(IEnumerable).IsAssignableFrom(multi.Type) == false)
                throw Error.InExpressionMustBeCollection(Token);

            var elementType = Utility.GetElementType(multi.Type);
            Debug.Assert(elementType != multi.Type);
            expr = Expression.Call(typeof(Enumerable), "Contains", new[] { elementType }, multi, expr);

            return expr;
        }

        private Expression ParseMultiSet()
        {
            //{1, 2, 3}
            Expression expr;
            if (Token.Identity == TokenId.OpenCurlyBrace)
            {
                NextToken();
                expr = ParseMultiSetInner();

                ValidateToken(TokenId.CloseCurlyBrace, Res.CloseCurlyBraceOrCommaExpected);
                NextToken();

                return expr;
            }

            //MULTISET(1, 2, 3)
            if (!TokenIdentifierIs(Keyword.MultiSet.ToString()))
                throw ParseError(Res.TokenExpected, Keyword.MultiSet);

            NextToken();
            ValidateToken(TokenId.OpenParen);
            NextToken();

            expr = ParseMultiSetInner();

            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();

            return expr;
        }



        Expression ParseMultiSetInner()
        {
            var expr = ParseValue();
            var elementType = expr.Type;
            var items = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            items.Add(((ConstantExpression)expr).Value);

            while (Token.Identity == TokenId.Comma)
            {
                NextToken();

                expr = ParseValue();
                if (expr.Type != elementType)
                    throw Error.MultisetElemsAreNotTypeCompatible();

                var value = ((ConstantExpression)expr).Value;
                items.Add(value);
            }

            var arr = System.Array.CreateInstance(elementType, items.Count);
            for (var i = 0; i < arr.Length; i++)
                arr.SetValue(items[i], i);

            return Expression.Constant(arr);
        }

        public IEnumerable<DynamicOrdering> ParseOrdering()
        {
            List<DynamicOrdering> orderings = new List<DynamicOrdering>();
            while (true)
            {
                Expression expr = ParseExpression();
                bool ascending = true;
                if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending"))
                {
                    NextToken();
                }
                else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending"))
                {
                    NextToken();
                    ascending = false;
                }
                orderings.Add(new DynamicOrdering { Selector = expr, Ascending = ascending });
                if (Token.Identity != TokenId.Comma) break;
                NextToken();
            }
            ValidateToken(TokenId.End, Res.SyntaxError);
            return orderings;
        }
    }
}
