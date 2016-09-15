
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ALinq.Dynamic.Parsers;

namespace ALinq.Dynamic
{
    static class ParserUtility
    {
        public static Expression ParseConstructor(this IParser parser, IEnumerable<string> namespaces)
        {
            var tmpToken = parser.Token();

            var className = parser.Token().Text;
            parser.NextToken();
            while (parser.Token().Identity == TokenId.Dot)
            {
                parser.NextToken();
                className = className + "." + parser.Token().Text;
                parser.NextToken();
            }



            var objectType = new TypeFinder().FindType(className, namespaces);
            if (objectType != null)
            {
                if (parser.Token().Identity != TokenId.OpenParen && parser.Token().Identity != TokenId.OpenCurlyBrace)
                    throw Error.Token1OrToken2Expected(parser.Token(), "(", "{");

                Expression[] args = new Expression[0];
                //if (objectType != null)
                //{
                if (parser.Token().Identity == TokenId.OpenParen)
                    args = parser.ParseArgumentList();

                var cons = ExpressionUtility.FindConstructor(objectType, args);
                if (cons == null)
                    throw Error.CanNotFindConstructor(parser.Token(), objectType, args.Select(o => o.Type).ToArray());

                var newExpr = Expression.New(cons, args);
                if (parser.Token().Identity == TokenId.OpenCurlyBrace)
                {

                    var expr = parser.ParseMemberInit(newExpr);

                    return expr;
                }

                return newExpr;
            }

            parser.TokenCursor.MoveTo(tmpToken.Position);
            return null;
        }

        public static Expression ParseMemberAccess(this IParser parser, Expression instance)
        {
            Debug.Assert(instance != null);
            var type = instance.Type;

            var errorPos = parser.Token().Position;
            string id = parser.Token().GetIdentifier();
            parser.NextToken();

            Expression expr = null;
            if (parser.Token().IsMethod)
            {
                expr = parser.ParseMethodAccess(type, instance, id);
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

            parser.TokenCursor.MoveTo(errorPos);
            return null;
        }

       

        public static Type ParseType(this IParser parser, IEnumerable<string> namespaces)
        {
            return ParseType(parser, namespaces, false);
        }

        public static Type ParseType(this IParser parser, IEnumerable<string> namespaces, bool throwException)
        {
            var tmpToken = parser.Token();

            string typeName = parser.Token().GetIdentifier();
            parser.NextToken();
            while (parser.Token().Identity == TokenId.Dot)
            {
                parser.NextToken();
                typeName = typeName + "." + parser.Token().GetIdentifier();
                parser.NextToken();
            }

            if (namespaces == null)
                namespaces = new string[0];

            var objectType = new TypeFinder().FindType(typeName,namespaces);
            if (objectType != null)
                return objectType;

            if (throwException)
                throw Error.CannotResolveNameToType(tmpToken, typeName);

            parser.TokenCursor.MoveTo(tmpToken.Position);
            return null;
        }
        #region MyRegion
        //public static Type ParseType(this IParser parser, IEnumerable<string> namespaces, bool throwException)
        //{
        //    var tmpToken = parser.Token();

        //    string typeName = parser.Token().GetIdentifier();
        //    parser.NextToken();

        //    string methodName = null;
        //    if (parser.Token().Identity == TokenId.Dot)
        //    {
        //        parser.NextToken();
        //        methodName = parser.Token().Text;
        //        parser.NextToken();
        //    }

        //    while (parser.Token().Identity == TokenId.Dot)
        //    {
        //        parser.NextToken();
        //        typeName = typeName + "." + methodName;
        //        methodName = parser.Token().GetIdentifier();

        //        parser.NextToken();
        //    }

        //    if(parser.Token().Identity != TokenId.OpenParen)
        //    {
        //        typeName = typeName + "." + methodName;
        //        methodName = null;
        //    }

        //    if (namespaces == null)
        //        namespaces = new string[0];

        //    var objectType = new TypeFinder(namespaces).FindType(typeName);
        //    if (objectType != null)
        //        return objectType;

        //    if (throwException)
        //        throw Error.CannotResolveNameToType(tmpToken, typeName);

        //    parser.TokenCursor.MoveTo(tmpToken.Position);
        //    return null;
        //} 
        #endregion

        private static Expression ParseMethodAccess(this IParser parser, Type type, Expression instance, string methodName)
        {

            Expression[] args = null;
            Func<Expression[]> getArgs = delegate()
                                           {
                                               args = parser.ParseArgumentList();
                                               return args;
                                           };

            var errorPos = parser.Token().Position;

            MethodBase mb;
            var methodsCount = ExpressionUtility.FindMethod(type, methodName, instance == null, getArgs, out mb);
            switch (methodsCount)
            {
                case 0:
                    return null;
                //throw Error.ParseError(errorPos, Res.NoApplicableMethod, methodName, TypeUtility.GetTypeName(type));
                case 1:
                    MethodInfo method = (MethodInfo)mb;
                    if (method.ReturnType == typeof(void))
                        throw Error.ParseError(errorPos, Res.MethodIsVoid, methodName, TypeUtility.GetTypeName(method.DeclaringType));


                    Debug.Assert(args != null);
                    return Expression.Call(instance, (MethodInfo)method, args);
                default:
                    throw Error.ParseError(errorPos, Res.AmbiguousMethodInvocation, methodName, TypeUtility.GetTypeName(type));
            }
        }



        public static Expression ParseMemberInit(this IParser parser, NewExpression newExpr)
        {
            Type objectType = newExpr.Type;
            //var Token = parser.TokenCursor.Current;
            parser.Token().Validate(TokenId.OpenCurlyBrace, Error.TokenExpected(parser.Token(), "{"));
            parser.TokenCursor.NextToken();

            //var preIt = parser.ParseContext[Constants.It];
            //parser.ParseContext[Constants.It] = newExpr;
            var args = parser.Token().Identity == TokenId.CloseCurlyBrace ? new MemberBinding[0] :
                                                              ParseMemberBindings(parser, objectType);
            // parser.ParseContext[Constants.It] = preIt;

            parser.Token().Validate(TokenId.CloseCurlyBrace, Error.TokenExpected(parser.Token(), "}"));
            parser.TokenCursor.NextToken();

            //var newExpr = Expression.New(objectType);
            var memberInit = Expression.MemberInit(newExpr, args);
            return memberInit;

        }

        public static Expression ParseIntegerLiteral(this IParser parser)
        {
            parser.Token().Validate(TokenId.IntegerLiteral);
            string text = parser.Token().Text;

            char last = text[text.Length - 1];
            char? lastPrevious = null;
            if (text.Length > 1)
                lastPrevious = text[text.Length - 2];

            if ((lastPrevious == LiteralPostfix.UnsignedUpper || lastPrevious == LiteralPostfix.UnsignedLower) &&
                (last == LiteralPostfix.LongUpper || last == LiteralPostfix.LongLower))
            {
                ulong value;
                if (!ulong.TryParse(text.Substring(0, text.Length - 2), out value))
                    throw Error.ParseError(parser.Token().Position, Res.InvalidIntegerLiteral, text);

                parser.NextToken();
                return CreateLiteral(value, text);
            }

            if (last == LiteralPostfix.UnsignedUpper || last == LiteralPostfix.UnsignedLower)
            {
                uint value;
                if (!uint.TryParse(text.Substring(0, text.Length - 1), out value))
                    throw Error.ParseError(parser.Token().Position, Res.InvalidIntegerLiteral, text);

                parser.NextToken();
                return CreateLiteral(value, text);
            }

            if (last == LiteralPostfix.LongUpper || last == LiteralPostfix.LongLower)
            {
                long value;
                if (!long.TryParse(text.Substring(0, text.Length - 1), out value))
                    throw Error.ParseError(parser.Token().Position, Res.InvalidIntegerLiteral, text);

                parser.NextToken();
                return CreateLiteral(value, text);
            }

            int int32;
            if (!int.TryParse(text, out int32))
                throw Error.ParseError(parser.Token().Position, Res.InvalidIntegerLiteral, text);

            parser.NextToken();
            return CreateLiteral(int32, text);

        }

        private static ConstantExpression CreateLiteral(object value, string text)
        {
            ConstantExpression expr = Expression.Constant(value);
            //literals.Add(expr, text);
            return expr;
        }

        //public static ConstantExpression ParseStringLiteral(this IParser parser)
        //{
        //    parser.Token().Validate(TokenId.StringLiteral);
        //    char quote = parser.Token().Text[0];
        //    string s = parser.Token().Text.Substring(1, parser.Token().Text.Length - 2);
        //    int start = 0;
        //    while (true)
        //    {
        //        int i = s.IndexOf(quote, start);
        //        if (i < 0) break;
        //        s = s.Remove(i, 1);
        //        start = i + 1;
        //    }
        //    parser.NextToken();

        //    ConstantExpression expr = Expression.Constant(s);
        //    return expr;
        //}

        internal static IEnumerable<MemberBinding> ParseMemberBindings(this IParser parser, Type objectType)
        {

            var memberBindings = new List<MemberBinding>();
            //var token = parser.Token();
            while (parser.Token().Identity != TokenId.End)
            {
                string propName = null;
                var expr = parser.ParseExpression();
                if (expr == null)
                    throw Error.UnknownIdentifier(parser.Token());

                if (expr.NodeType == ExpressionType.MemberAccess)
                    propName = ((MemberExpression)expr).Member.Name;

                if (parser.Token().Keyword == Keyword.As)
                {
                    parser.NextToken();
                    propName = parser.Token().GetIdentifier();
                    parser.NextToken();
                }

                var errorToken = parser.TokenCursor.Current;
                if (propName == null)
                    throw Error.MissingAsClause(errorToken);

                var pf = TypeUtility.FindPropertyOrField(objectType, propName, false);
                if (pf == null)
                    throw Error.NoPublicPropertyOrField(errorToken, propName, objectType);

                if (pf.MemberType == MemberTypes.Property)
                {
                    var p = ((PropertyInfo)pf);

                    if (p.CanWrite == false)
                        throw Error.PropertyNoSetter(parser.Token(), ((PropertyInfo)pf));

                    var setMethod = p.GetSetMethod();
                    if (setMethod == null || setMethod.IsPublic == false)
                        throw Error.RequirePublicPropertySetter(parser.Token(), p);
                }
                else
                {
                    var f = (FieldInfo)pf;
                    if (f.IsPublic == false)
                        throw Error.RequirePublicField(parser.Token(), f);
                }




                var mb = Expression.Bind(pf, expr);
                memberBindings.Add(mb);

                if (parser.Token().Identity == TokenId.CloseCurlyBrace)
                    break;

                if (parser.Token().Identity == TokenId.Comma)
                {
                    parser.NextToken();
                    continue;
                }

                throw Error.ParseError(errorToken, () => Res.CloseParenOrCommaExpected);
            }

            //var memberInit = Expression.MemberInit(newExpr, memberBindings);
            //return memberInit;
            return memberBindings;
        }

        public static Expression[] ParseArgumentList(this IParser parser)
        {
            parser.Token().Validate(TokenId.OpenParen, Res.OpenParenExpected);
            parser.NextToken();
            Expression[] args = parser.Token().Identity != TokenId.CloseParen ? ParseArguments(parser) : new Expression[0];
            parser.Token().Validate(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            parser.NextToken();
            return args;
        }

        private static Expression[] ParseArguments(this IParser parser)
        {
            List<Expression> argList = new List<Expression>();
            while (true)
            {
                var arg = parser.ParseExpression();
                Debug.Assert(arg != null);
                argList.Add(arg);

                var token = parser.TokenCursor.Current;
                if (token.Identity != TokenId.Comma)
                    break;

                parser.TokenCursor.NextToken();
            }
            return argList.ToArray();
        }



        static Token Token(this IParser parser)
        {
            return parser.TokenCursor.Current;
        }

        static void NextToken(this IParser parser)
        {
            parser.TokenCursor.NextToken();
        }
    }
}

