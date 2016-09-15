using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using E = ALinq.Dynamic.ExpressionUtility;

namespace ALinq.Dynamic.Parsers
{
    class UnaryExpressionParser : IParser
    {
        private TokenCursor tokenCursor;
        private Dictionary<string, object> symbols;

        #region predefinedTypes

        private Dictionary<string, Type> predefinedTypes = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase)
        {
            { "object", typeof(object) },
            { "bool", typeof(bool) },
            { "char", typeof(char) },
            { "string", typeof(string) },
            { "sbyte", typeof(sbyte) },
            { "byte", typeof(byte) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) },
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "float", typeof(float) },
            { "double", typeof(double) },
            { "decimal", typeof(decimal) },
            { "DateTime", typeof(DateTime) },
            { "TimeSpan", typeof(TimeSpan) },
            { "Guid", typeof(Guid) },
            { "Math", typeof(Math) },
            { "Convert", typeof(Convert) },
            { "Binary", typeof(byte[]) },
            //{ "X", typeof(byte[]) }
        };


        #endregion

        private Dictionary<string, object> keywords;

        #region Methods
        private static class Methods
        {
            private static MethodInfo newGuid;
            private static MethodInfo subString;
            private static MethodInfo concat;

            public static MethodInfo NewGuid
            {
                get
                {
                    if (newGuid == null)
                    {
                        newGuid = typeof(Guid).GetMethod("NewGuid");
                    }
                    return newGuid;
                }
            }

            public static MethodInfo Substring
            {
                get
                {
                    if (subString == null)
                        subString = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) });

                    return subString;
                }
            }

            public static MethodInfo Concat
            {
                get
                {
                    if (concat == null)
                    {
                        concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                    }
                    return concat;
                }
            }
        }
        #endregion

        private UnaryExpressionParser()
            : base()
        {
            this.symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            //this.parseContext = new Dictionary<string, object>() { };

            this.symbols.Add("true", Literals.True);
            this.symbols.Add("false", Literals.False);
            this.symbols.Add("null", Literals.Null);

            this.ProcessKeywords();

        }

        public UnaryExpressionParser(TokenCursor tokenCursor)
            : this(tokenCursor, null)
        {

        }


        public UnaryExpressionParser(TokenCursor tokenCursor, IEnumerable<ObjectParameter> parameters)
            : this()
        {
            if (tokenCursor == null)
                throw new ArgumentNullException("tokenCursor");

            this.tokenCursor = tokenCursor;

            if (parameters != null)
                ProcessParameters(parameters);

        }

        protected Token Token
        {
            get
            {
                return this.tokenCursor.Current;
            }
        }


        public TokenCursor TokenCursor
        {
            get
            {
                return this.tokenCursor;
            }
        }

        protected void NextToken()
        {
            this.tokenCursor.NextToken();
        }

        protected virtual object GetSymbol(string symbolName)
        {
            Debug.Assert(symbols != null);
            object value;
            if (symbols.TryGetValue(symbolName, out value))
                return value;


            return null;
        }

        public virtual Expression ParseExpression()
        {
            Expression expr = ParsePrimaryStart();
            while (true)
            {
                if (Token.Identity == TokenId.Dot)
                {
                    NextToken();

                    Debug.Assert(expr != null);
                    expr = ParseMemberAccess(expr);
                }
                else if (Token.Identity == TokenId.OpenBracket)
                {
                    Debug.Assert(expr != null);
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
                return this.ParseIntegerLiteral();
            }
            if (Token.Identity == TokenId.RealLiteral)
            {
                return ParseRealLiteral();
            }
            if (Token.Identity == TokenId.BinaryLiteral)
            {
                return ParseBinaryLiteral();
            }
            if (Token.Identity == TokenId.OpenParen)
            {
                return ParseParenExpression();
            }
            if (Token.Identity == TokenId.OpenCurlyBrace)
            {
                return ParseMultiSet();
            }
            if (Token.Identity == TokenId.GuidLiteral)
            {
                return ParseGuidLiteral();
            }
            throw Error.GenericSyntaxError(Token);
        }

        private Expression ParseValue()
        {
            switch (Token.Identity)
            {
                case TokenId.StringLiteral:
                    return this.ParseStringLiteral();
                case TokenId.Sharp:
                    return ParseDateTimeLiteral();
                case TokenId.IntegerLiteral:
                    return this.ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return ParseRealLiteral();
                case TokenId.BinaryLiteral:
                    return ParseBinaryLiteral();
                case TokenId.GuidLiteral:
                    return this.ParseGuidLiteral();
                default:
                    //if(Token.Id
                    throw Error.ParseError(Token.Position, Res.ExpressionExpected);
            }
        }

        private Expression ParseGuidLiteral()
        {
            Token.Validate(TokenId.GuidLiteral);
            var str = Token.Text.Substring(2, Token.Text.Length - 3);
            var value = new Guid(str);
            NextToken();
            return Expression.Constant(value);
        }

        private Expression ParseBinaryLiteral()
        {
            Token.Validate(TokenId.BinaryLiteral);
            Debug.Assert(Token.Text.Length >= 3);
            Debug.Assert(Token.Text[0] == 'X');
            Debug.Assert(Token.Text[1] == '\'');
            Debug.Assert(Token.Text[Token.Text.Length - 1] == '\'');
            var str = Token.Text.Substring(2, Token.Text.Length - 3);
            var value = StringToByte(str);
            NextToken();
            return Expression.Constant(value);
        }

        private ConstantExpression ParseStringLiteral()
        {
            Token.Validate(TokenId.StringLiteral);
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

            return CreateLiteral(s);
        }

        private Expression ParseDateTimeLiteral()
        {
            Token.Validate(TokenId.Sharp);
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
            return CreateLiteral(Convert.ToDateTime(s));
        }



        private Expression ParseRealLiteral()
        {
            Token.Validate(TokenId.RealLiteral);
            string text = Token.Text;
            object value = null;
            char last = text[text.Length - 1];
            if (last == LiteralPostfix.FloatUpper || last == LiteralPostfix.FloatLower)
            {
                float f;
                if (Single.TryParse(text.Substring(0, text.Length - 1), out f))
                    value = f;
            }
            else if (last == LiteralPostfix.DecimalUpper || last == LiteralPostfix.DecimalLower)
            {
                decimal m;
                if (Decimal.TryParse(text.Substring(0, text.Length - 1), out m))
                    value = m;
            }
            else
            {
                double d;
                if (Double.TryParse(text, out d)) value = d;
            }
            if (value == null)
                throw Error.ParseError(Token.Position, Res.InvalidRealLiteral, text);

            NextToken();
            return CreateLiteral(value);
        }

        private ConstantExpression CreateLiteral(object value)
        {
            ConstantExpression expr = Expression.Constant(value);
            return expr;
        }

        private Expression ParseParenExpression()
        {
            Debug.Assert(Token.Identity == TokenId.OpenParen);
            Token.Validate(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            Expression e = this.ParseExpression();
            Token.Validate(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
            NextToken();
            return e;
        }

        protected virtual Expression ParseIdentifier()
        {
            Token.Validate(TokenId.Identifier);

            object value;
            if (Token.Function != Function.None)
            {
                return ParseFunction();
            }

            if (keywords.TryGetValue(Token.Text, out value))
            {
                if (value is Type)
                    return ParseTypeAccess((Type)value);
            }

            if ((value = GetSymbol(Token.Text)) != null)
            {
                Expression expr;
                if (value is Expression)
                {
                    expr = (Expression)value;
                }
                else if (value is IQueryable)
                {
                    expr = ((IQueryable)value).Expression;
                }
                else
                {
                    expr = Expression.Constant(value);
                }

                NextToken();
                return expr;
            }

            if (Token.Text[0] == '@')
                throw Error.ParameterWasNotDefined(Token, Token.Text.Substring(1));

            return null;

        }

        private Expression ParseFunction()
        {
            Function function = Token.Function;
            switch (function)
            {
                case Function.Cast:
                    return this.ParseCast();

                #region 时间函数

                case Function.Day:
                case Function.DayOfYear:
                case Function.Hour:
                case Function.Minute:
                case Function.Month:
                case Function.Second:
                case Function.Year:
                    {
                        var op = Token;
                        NextToken();
                        var args = ParseArgumentList();
                        Debug.Assert(args != null);

                        if (args.Length != 1 || (args[0].Type != typeof(DateTime) && args[0].Type != typeof(string)))
                            throw Error.NoCanonicalFunctionOverloadMatch(op, function.ToString(),
                                                                         args.Select(o => o.Type));

                        var member = typeof(DateTime).GetProperty(function.ToString());
                        Debug.Assert(member != null);
                        //var instance = E.PromoteExpression(args[0], typeof(DateTime), false);
                        //if (instance == null)
                        //    throw Error.IncompatibleOperandsError(op.Text, args[0].Type, typeof(DateTime), op.Position);

                        //Expression.Call(typeof(Convert), "ChangeType", new Type[0], new[] { args[0], Expression.Constant(typeof(DateTime)) })
                        if (args[0].Type == typeof(string))
                        {
                            var c = Expression.Call(typeof(Convert), "ChangeType", new Type[0],
                                                    new[] { args[0], Expression.Constant(typeof(DateTime)) });
                            args[0] = Expression.Convert(c, typeof(DateTime));
                        }

                        var expr = Expression.MakeMemberAccess(args[0], member);
                        return expr;
                    }

                #endregion

                #region 位函数

                case Function.BitWiseAnd:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        var expr = Expression.And(args[0], args[1]);
                        return expr;
                    }
                case Function.BitWiseNot:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        var expr = Expression.Not(args[0]);
                        return expr;
                    }
                case Function.BitWiseOr:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        var expr = Expression.Or(args[0], args[1]);
                        return expr;
                    }
                case Function.BitWiseXor:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        var expr = Expression.ExclusiveOr(args[0], args[1]);
                        return expr;
                    }

                #endregion

                #region 数学函数

                case Function.Abs:
                case Function.Ceiling:
                case Function.Floor:
                case Function.Power:
                case Function.Round:
                    {
                        var math = typeof(Math);
                        var expr = ParseStaticMemberAccess(math);
                        return expr;
                    }

                case Function.PI:
                    {
                        NextToken();
                        Token.Validate(TokenId.OpenParen);

                        NextToken();
                        Token.Validate(TokenId.CloseParen);

                        var p = Expression.Constant(null);
                        var m = TypeUtility.FindPropertyOrField(typeof(Math), "PI", true);
                        Debug.Assert(m != null);

                        Expression exp = Expression.MakeMemberAccess(null, m);

                        NextToken();

                        return exp;
                    }

                #endregion

                case Function.MultiSet:
                    return this.ParseMultiSet();

                #region 字符串函数

                case Function.Concat:
                    {
                        MethodInfo concat = Methods.Concat;
                        var expr = ParseStaticMethod(concat);
                        return expr;
                    }
                case Function.Contains:
                case Function.EndsWith:
                case Function.IndexOf:
                case Function.StartsWith:
                    {
                        var methodName = function.ToString();
                        NextToken();
                        var args = ParseArgumentList();
                        //TODO:判断 args 长度为 2;
                        //TODO:判断 arg 的类型

                        var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });
                        var expr = Expression.Call(args[0], method, args[1]);

                        NextToken();
                        return expr;
                    }
                case Function.Left:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        //TODO:判斷參數為2
                        var method = Methods.Substring;
                        var expr = Expression.Call(args[0], method, Expression.Constant(0), args[1]);
                        return expr;
                    }
                case Function.Length:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        //TODO:判斷參數為1
                        var expr = ParseStringLength(args[0]);
                        return expr;
                    }
                case Function.LTrim:
                //case Function.Reverse:
                case Function.RTrim:
                case Function.ToLower:
                case Function.ToUpper:
                case Function.Trim:
                    {
                        MethodInfo method;
                        Expression[] args = new Expression[0];
                        switch (function)
                        {
                            case Function.LTrim:
                                {
                                    Expression<Func<string, string>> e = o => o.TrimStart();
                                    method = ((MethodCallExpression)e.Body).Method;
                                    args = new Expression[] { Expression.Constant(new char[0]) };
                                    break;
                                }
                            case Function.RTrim:
                                {
                                    Expression<Func<string, string>> e = o => o.TrimEnd();
                                    method = ((MethodCallExpression)e.Body).Method;
                                    args = new Expression[] { Expression.Constant(new char[0]) };
                                    break;
                                }

                            default:
                                string methodName = function.ToString();
                                method = typeof(string).GetMethod(methodName, new Type[0]);
                                Debug.Assert(method != null);

                                break;
                        }
                        NextToken();

                        //TODO:判断 args 长度为 1;
                        //TODO:判断 arg 的类型
                        var list = ParseArgumentList();
                        var instance = list[0];


                        var expr = Expression.Call(instance, method, args);

                        NextToken();
                        return expr;
                    }

                case Function.Replace:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        //TODO:判斷參數為3
                        Expression<Func<string, string>> e = o => o.Replace("", "");
                        var method = ((MethodCallExpression)e.Body).Method;
                        var expr = Expression.Call(args[0], method, new[] { args[1], args[2] });
                        return expr;
                    }
                case Function.Right:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        //TODO:判斷參數為2
                        var length = ParseStringLength(args[0]);
                        var index = Expression.MakeBinary(ExpressionType.Subtract, length, args[1]);
                        var method = Methods.Substring;
                        var expr = Expression.Call(args[0], method, index, args[1]);
                        return expr;
                    }
                case Function.Substring:
                    {
                        NextToken();
                        var args = ParseArgumentList();
                        //TODO:判斷參數為2，类型
                        Expression<Func<string, string>> e = o => o.Substring(0, 1);
                        var method = Methods.Substring;
                        var expr = Expression.Call(args[0], method, new[] { args[1], args[2] });
                        return expr;
                    }


                case Function.Truncate:
                    {
                        var expr = ParseTruncate();
                        return expr;
                    }
                #endregion

                #region 其它规范函数
                case Function.NewGuid:
                    {
                        NextToken();
                        Token.Validate(TokenId.OpenParen);
                        NextToken();
                        Token.Validate(TokenId.CloseParen);
                        NextToken();

                        var expr = Expression.Call(null, Methods.NewGuid);
                        return expr;
                    }
                case Function.IIF:
                    {
                        return this.ParseIif();
                    }
                #endregion
            }

            throw new NotImplementedException(function.ToString());
        }



        //=============================================================
        // 說明：將 Truncate 轉換為 Round 函數計算。
        // 公式：
        // var dec = 5 / Math.Pow(10, digits + 1)
        // var result = Math.Round(value + dec, digits) - dec * 2
        // 其中：value 和 digits 是輸入的參數，result 是返回值。
        //==============================================================
        private Expression ParseTruncate()
        {
            var errorToken = Token;
            NextToken();
            Token.Validate(TokenId.OpenParen);
            var args = ParseArgumentList();
            Expression arg0 = args[0];
            Expression arg1 = args[1];

            //=========================================================
            // 構造 5 / Math.Pow(10, digits + 1) 的 Expression 為 dec
            Expression left;
            Expression right;
            left = Expression.Constant(10d, typeof(double));
            right = Expression.Add(Expression.Convert(arg1, typeof(double)), Expression.Constant(1d, typeof(double)));
            //E.CheckAndPromoteOperands(typeof(IArithmeticSignatures), "F", ref left, ref right, errorToken.Position);
            var pow = ParsePower(new[] { left, right }, errorToken);

            left = Expression.Constant(5d, typeof(double));
            right = pow;
            //E.CheckAndPromoteOperands(typeof(IArithmeticSignatures), "F", ref left, ref right, errorToken.Position);
            var dec = Expression.Divide(left, right);
            //==========================================================

            var roundArgs = new[] { Expression.Add(arg0, dec), args[1] };
            var round = ParseRound(roundArgs, errorToken);
            var expr = Expression.Subtract(Expression.Subtract(round, dec), dec);
            return expr;
        }

        private Expression ParsePower(Expression[] args, Token errorToken)
        {
            MethodBase method;
            var expr = this.ParseStaticMemberAccess(typeof(Math), "Pow", args, errorToken);
            return expr;
        }

        private Expression ParseStaticMethod(MethodInfo method)
        {
            Debug.Assert(method.ReturnType != typeof(void));

            NextToken();
            if (Token.Identity != TokenId.OpenParen)
                throw Error.ParseError(Token.Position, Res.TokenExpected, TokenId.OpenParen);

            Expression[] args = ParseArgumentList();


            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
                throw Error.ParseError(Token.Position, Res.NoMatchingMethod, method.Name);

            return Expression.Call(null, method, args);
        }

        private Expression ParseStaticMemberAccess(Type type)
        {
            var errorToken = Token;
            string id = Token.GetIdentifier();
            NextToken();
            if (Token.Identity != TokenId.OpenParen)
            {
                //throw Error.TokenExpected(Token.Position, TokenId.OpenParen);
                var pf = TypeUtility.FindPropertyOrField(type, id, true);
                if (pf == null)
                    throw Error.NoPublicPropertyOrField(errorToken, id, type);

                return Expression.MakeMemberAccess(null, pf);
            }

            Expression[] args = ParseArgumentList();
            MethodBase mb;

            string methodName;
            switch (id.ToUpper())
            {
                case "POWER":
                    methodName = "POW";
                    break;
                default:
                    methodName = id;
                    break;
            }

            var methodsCount = ExpressionUtility.FindMethod(type, methodName, true, args, out mb);
            switch (methodsCount)
            {
                case 0:
                    throw Error.ParseError(errorToken, () => Res.NoApplicableMethod, id, TypeUtility.GetTypeName(type));
                case 1:
                    MethodInfo method = (MethodInfo)mb;
                    if (method.ReturnType == typeof(void))
                        throw Error.ParseError(errorToken, Res.MethodIsVoid, id, TypeUtility.GetTypeName(method.DeclaringType));

                    return Expression.Call(null, method, args);
                default:
                    throw Error.ParseError(errorToken, Res.AmbiguousMethodInvocation, id, TypeUtility.GetTypeName(type));
            }


        }

        private Expression ParseStaticMemberAccess(Type type, string methodName,
                                                   Expression[] args, Token errorToken)
        {
            MethodBase mb;

            var methodsCount = ExpressionUtility.FindMethod(type, methodName, true, args, out mb);
            switch (methodsCount)
            {
                case 0:
                    throw Error.ParseError(errorToken, () => Res.NoApplicableMethod, methodName, TypeUtility.GetTypeName(type));
                case 1:
                    MethodInfo method = (MethodInfo)mb;
                    if (method.ReturnType == typeof(void))
                        throw Error.ParseError(errorToken, Res.MethodIsVoid, methodName, TypeUtility.GetTypeName(method.DeclaringType));

                    return Expression.Call(null, method, args);
                default:
                    throw Error.ParseError(errorToken, Res.AmbiguousMethodInvocation, methodName, TypeUtility.GetTypeName(type));
            }


        }


        private Expression ParseRound(Expression[] args, Token errorToken)
        {
            MethodBase method;
            var expr = this.ParseStaticMemberAccess(typeof(Math), "Round", args, errorToken);
            return expr;
        }

        private Expression ParseStringLength(Expression instance)
        {
            var p = typeof(string).GetProperty("Length");
            var expr = Expression.MakeMemberAccess(instance, p);
            return expr;
        }



        private Expression ParseCast()
        {
            NextToken();
            Token.Validate(TokenId.OpenParen);

            NextToken();

            var expr1 = this.ParseExpression();

            Token.Validate(Keyword.As, Error.KeywordExpected);
            NextToken();

            //从 keywords 获取需要转换的类型
            Type targetType;
            object value;
            if (!keywords.TryGetValue(Token.Text, out value) || (targetType = value as Type) == null)
            {
                throw Error.GenericSyntaxError(Token);//TODO:显示异常信息。
            }

            var typeCode = Type.GetTypeCode(targetType);
            var methodName = "To" + typeCode;
            MethodInfo method = typeof(Convert).GetMethod(methodName, new[] { expr1.Type });

            NextToken();
            Token.Validate(TokenId.CloseParen);

            NextToken();

            var expr = Expression.Call(null, method, expr1);
            return expr;
        }

        private Expression ParseIif()
        {
            var errorPos = Token.Position;
            NextToken();
            Expression[] args = ParseArgumentList();
            if (args.Length != 3)
                throw Error.ParseError(errorPos, Res.IifRequiresThreeArgs);

            var expr = ExpressionUtility.GenerateConditional(args[0], args[1], args[2], errorPos);
            return expr;
        }

        private Expression ParseTypeAccess(Type type)
        {
            int errorPos = Token.Position;
            NextToken();

            if (Token.Identity == TokenId.Question)
            {
                if (!type.IsValueType || TypeUtility.IsNullableType(type))
                    throw Error.ParseError(errorPos, Res.TypeHasNoNullableForm, TypeUtility.GetTypeName(type));
                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }

            if (Token.Identity == TokenId.StringLiteral)
            {
                var expr = ParseStringLiteral();
                return this.ParseValueByType(type, expr);
            }

            if (Token.Identity == TokenId.OpenParen)
            {
                Expression[] args = ParseArgumentList();
                MethodBase method;
                switch (ExpressionUtility.FindBestMethod(type.GetConstructors(), args, out method))
                {
                    case 0:
                        if (args.Length == 1)
                            return GenerateConversion(args[0], type, errorPos);
                        throw Error.ParseError(errorPos, Res.NoMatchingConstructor, TypeUtility.GetTypeName(type));
                    case 1:
                        return Expression.New((ConstructorInfo)method, args);
                    default:
                        throw Error.ParseError(errorPos, Res.AmbiguousConstructorInvocation, TypeUtility.GetTypeName(type));
                }
            }

            if (Token.Identity == TokenId.CloseParen)
            {
                NextToken();
                var expr = this.ParseExpression();
                expr = Expression.Convert(expr, type);
                return expr;
            }
            Token.Validate(TokenId.Dot, Res.OpenParenExpected);
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
                        var bytes = StringToByte(value);
                        return Expression.Constant(bytes);
                    }

                    break;

            }
            throw Error.TypeNotSupported(Token, type);
        }

        private static byte[] StringToByte(string strValue)
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

        private Expression GenerateConversion(Expression expr, Type type, int errorPos)
        {
            Type exprType = expr.Type;
            if (exprType == type) return expr;
            if (exprType.IsValueType && type.IsValueType)
            {
                if ((TypeUtility.IsNullableType(exprType) || TypeUtility.IsNullableType(type)) &&
                    TypeUtility.GetNonNullableType(exprType) == TypeUtility.GetNonNullableType(type))
                    return Expression.Convert(expr, type);
                if ((TypeUtility.IsNumericType(exprType) || TypeUtility.IsEnumType(exprType)) &&
                    (TypeUtility.IsNumericType(type)) || TypeUtility.IsEnumType(type))
                    return Expression.ConvertChecked(expr, type);
            }
            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
                exprType.IsInterface || type.IsInterface)
                return Expression.Convert(expr, type);
            throw Error.ParseError(errorPos, Res.CannotConvertValue,
                             TypeUtility.GetTypeName(exprType), TypeUtility.GetTypeName(type));
        }


        private Expression ParseMemberAccess(Expression instance)
        {
            Debug.Assert(instance != null);

            var type = instance.Type;

            var errorPos = Token.Position;
            string id = Token.GetIdentifier();
            NextToken();

            Expression expr = null;
            if (Token.Identity == TokenId.OpenParen)
            {
                expr = this.ParseMethodAccess(instance, id, type);
            }
            else
            {
                MemberInfo member = TypeUtility.FindPropertyOrField(type, id, false);
                if (member != null)
                {
                    Debug.Assert(instance != null);
                    expr = Expression.MakeMemberAccess(instance, member);
                }
            }

            if (expr != null)
                return expr;

            tokenCursor.MoveTo(errorPos);
            return null;

            //throw Error.UnknownPropertyOrField(Token, id, TypeUtility.GetTypeName(type));
        }

        private Expression ParseMethodAccess(Expression instance, string methodName, Type type)
        {
            if (type == null)
                type = instance.Type;

            Debug.Assert(instance.Type == type);

            Expression[] args = ParseArgumentList();
            MethodBase mb;

            var errorPos = Token.Position;
            var methodsCount = ExpressionUtility.FindMethod(type, methodName, instance == null, args, out mb);
            switch (methodsCount)
            {
                case 0:
                    throw Error.ParseError(errorPos, Res.NoApplicableMethod, methodName, TypeUtility.GetTypeName(type));
                case 1:
                    MethodInfo method = (MethodInfo)mb;

                    if (method.ReturnType == typeof(void))
                        throw Error.ParseError(errorPos, Res.MethodIsVoid, methodName, TypeUtility.GetTypeName(method.DeclaringType));

                    return Expression.Call(instance, (MethodInfo)method, args);
                default:
                    throw Error.ParseError(errorPos, Res.AmbiguousMethodInvocation, methodName, TypeUtility.GetTypeName(type));

            }
        }


        private Expression ParseElementAccess(Expression expr)
        {

            int errorPos = Token.Position;
            if (Token.Text[0] != '[')
                throw Error.ParseError(Token, Res.OpenBracketExpected);

            NextToken();
            Expression[] args;
            if (this.Token.Is(TokenId.Identifier))
            {
                args = new[] { Expression.Constant(Token.Text) };
                NextToken();
            }
            else
                args = ParseArguments();

            Token.Validate(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
            NextToken();
            if (expr.Type.IsArray)
            {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                    throw Error.ParseError(errorPos, Res.CannotIndexMultiDimArray);
                Expression index = ExpressionUtility.PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                    throw Error.ParseError(errorPos, Res.InvalidIndex);
                return Expression.ArrayIndex(expr, index);
            }
            else
            {
                MethodBase mb;
                switch (TypeUtility.FindIndexer(expr.Type, args, out mb))
                {
                    case 0:
                        throw Error.ParseError(errorPos, Res.NoApplicableIndexer,
                                         TypeUtility.GetTypeName(expr.Type));
                    case 1:
                        return Expression.Call(expr, (MethodInfo)mb, args);
                    default:
                        throw Error.ParseError(errorPos, Res.AmbiguousIndexerInvocation,
                                         TypeUtility.GetTypeName(expr.Type));
                }
            }
        }

        private Expression ParseMultiSet()
        {
            //{1, 2, 3}
            Expression expr;
            if (Token.Identity == TokenId.OpenCurlyBrace)
            {
                NextToken();
                expr = ParseMultiSetInner();

                Token.Validate(TokenId.CloseCurlyBrace, Res.CloseCurlyBraceOrCommaExpected);
                NextToken();

                return expr;
            }

            //MULTISET(1, 2, 3)
            if (Token.Function != Function.MultiSet)
                throw Error.ParseError(Token.Position, Res.TokenExpected, Function.MultiSet);

            NextToken();
            Token.Validate(TokenId.OpenParen);
            NextToken();

            expr = ParseMultiSetInner();

            Token.Validate(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();

            return expr;
        }

        private Expression ParseMultiSetInner()
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

        private Expression[] ParseArgumentList()
        {
            Token.Validate(TokenId.OpenParen, Res.OpenParenExpected);
            this.NextToken();
            Expression[] args = Token.Identity == TokenId.CloseParen ? new Expression[0] : ParseArguments();
            Token.Validate(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            this.NextToken();
            return args;
        }

        private Expression[] ParseArguments()
        {
            List<Expression> argList = new List<Expression>();
            while (true)
            {
                var arg = ParseExpression();
                argList.Add(arg);

                if (Token.Identity != TokenId.Comma) break;
                this.NextToken();
            }
            return argList.ToArray();
        }


        private void ProcessParameters(IEnumerable<ObjectParameter> items)
        {
            foreach (var parameter in items)
            {
                AddSymbol("@" + parameter.Name, parameter.Value);
            }
        }

        private void AddSymbol(string name, object value)
        {
            if (symbols.ContainsKey(name))
                throw Error.ParseError(Token.Position, Res.DuplicateIdentifier, name);
            symbols.Add(name, value);
        }

        private void ProcessKeywords()
        {
            Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in predefinedTypes)
                d.Add(item.Key, item.Value);

            Array keywordValues = Enum.GetValues(typeof(Keyword));
            foreach (object value in keywordValues)
            {
                string name = Enum.GetName(typeof(Keyword), value);
                d.Add(name, value);
            }
            ;

            this.keywords = d;
        }
    }
}
