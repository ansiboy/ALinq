using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ALinq.Dynamic.Parsers;


namespace ALinq.Dynamic
{
    class Error
    {
        public static Exception ParseError(Token errorToken, string format, params object[] args)
        {
            return ParseError(errorToken.Position, format, args);
        }

        public static Exception ParseError(Token errorToken, Expression<Func<string>> format, params object[] args)
        {
            //return ParseError(pos.Sequence, format, args);
            return new EntitySqlException(format, errorToken, args);
        }

        public static Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
        }

        public static Exception AliasNameAlreadyUsed(Token errorToken, string aliasName)
        {
            return new EntitySqlException(() => Res.AliasNameAlreadyUsed, errorToken, aliasName);
        }

        public static Exception ExpressionTypeMustBeQueryable()
        {
            var str = Res.ExpressionTypeMustBeQueryable;
            return new System.Exception(str);
        }

        public static Exception MemberTypeMismatch(MemberInfo member1, MemberInfo member2)
        {
            var args = new[] { member1.DeclaringType.FullName, member1.Name, member2.DeclaringType.FullName, member2.Name };
            var msg = String.Format(Res.MemberTypeMismatch, args);
            return new System.Exception(msg);
        }

        public static Exception ConstantInvalidType()
        {
            var msg = Res.Cqt_Constant_InvalidConstantType;
            return new System.Exception(msg);
        }

        public static Exception MultisetElemsAreNotTypeCompatible()
        {
            var msg = Res.MultisetElemsAreNotTypeCompatible;
            return new System.Exception(msg);
        }

        public static Exception NotPropertyOrField(string memberName)
        {
            var msg = string.Format(Res.ELinq_NotPropertyOrField, memberName);
            return new Exception(msg);
        }

        public static Exception MemberMustBeIQueryable(MemberInfo member)
        {
            var msg = string.Format(Res.ELinq_MemberMustBeIQueryable, member.Name, member.DeclaringType.FullName);
            return new Exception(msg);
        }

        public static Exception MemberCannotBeNull(MemberInfo member)
        {
            var msg = string.Format(Res.ELinq_MemberCannotBeNull, member.Name, member.DeclaringType.FullName);
            return new Exception(msg);
        }

        //public static Exception ExpressionMustBeIQueryable()
        //{
        //    var msg = Strings.ELinq_ExpressionMustBeIQueryable;
        //    return new Exception(msg);
        //}

        public static Exception ExpressionCannotBeNull()
        {
            var ms = Res.ExpressionCannotBeNull;
            return new Exception(ms);
        }

        public static Exception ArgumentNull(string parameterName)
        {
            return new ArgumentNullException(parameterName);
        }

        public static Exception InvalidSelectValueList(Token token)
        {
            //var msg = Strings.InvalidSelectValueList;
            //return new Exception(msg);
            return new EntitySqlException(() => Res.InvalidSelectValueList, token);
        }

        public static EntitySqlException InvalidGroupIdentifierReference(TokenPosition errorPos, string identifier)
        {
            return new EntitySqlException(() => Res.InvalidGroupIdentifierReference, errorPos, identifier);
        }

        public static EntitySqlException ExpressionTypeMustBeBoolean(Token token, string predicate)
        {
            return new EntitySqlException(() => Res.ExpressionTypeMustBeBoolean, token);
        }

        public static EntitySqlException GenericSyntaxError(Token errorToken)
        {
            return new EntitySqlException(() => Res.GenericSyntaxError, errorToken);
        }

        public static EntitySqlException GenericSyntaxError(Token errorToken, string errorMessage)
        {
            return new EntitySqlException(errorMessage, errorToken);
        }

        static string ErrorContext(TokenPosition pos)
        {
            return string.Format("{0} {1} {2}, {3} {4}.", Res.LocalizedNear, Res.LocalizedLine, pos.Line + 1,
                                Res.LocalizedColumn, pos.Column + 1);
        }

        public static Exception UnknownIdentifier(Token token)
        {
            return new EntitySqlException(() => Res.CouldNotResolveIdentifier, token, token.Text);
        }

        public static Exception ParameterWasNotDefined(Token token, string parameterName)
        {
            return new EntitySqlException(() => Res.ParameterWasNotDefined, token, parameterName);
        }

        public static Exception UnterminatedStringLiteral(TokenPosition pos)
        {
            return new EntitySqlException(() => Res.UnterminatedStringLiteral, pos);
        }

        public static Exception UnterminatedBinaryLiteral(TokenPosition pos)
        {
            return new EntitySqlException(() => Res.UnterminatedBinaryLiteral, pos);
        }

        public static Exception UnterminatedDateTimeLiteral(TokenPosition pos)
        {
            return new EntitySqlException(() => Res.UnterminatedStringLiteral, pos);
        }

        public static Exception TypeNotSupported(Token token, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return new EntitySqlException(() => Res.TypeNotSupported, token, type.Name);
        }

        public static Exception InExpressionMustBeCollection(Token token)
        {
            var errorContext = ErrorContext(token.Position);
            return new EntitySqlException(() => Res.InExpressionMustBeCollection, token);
        }

        public static Exception IncompatibleOperandsError(string opName, Type sourceType, Type targetType, int pos)
        {
            return Error.ParseError(pos, "Operator '{0}' incompatible with operand types '{1}' and '{2}'", new object[]
			{
				opName, 
			    TypeUtility.GetTypeName(sourceType), 
			    TypeUtility.GetTypeName(targetType)
			});
        }

        public static Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
        {
            return Error.ParseError(pos, "Operator '{0}' incompatible with operand types '{1}' and '{2}'", new object[]
			{
				opName, 
			    TypeUtility.GetTypeName(left.Type), 
			    TypeUtility.GetTypeName(right.Type)
			});
        }

        internal static Exception MemberMustBeIQueryable(Token token, MemberInfo m, Type dataContextType)
        {
            var errorContext = ErrorContext(token.Position);
            return new EntitySqlException(() => Res.ELinq_MemberMustBeIQueryable, token);
        }

        public static Exception TokenExpected(TokenPosition errorPos, TokenId id)
        {
            return new EntitySqlException(() => Res.TokenExpected, errorPos, id);
        }

        public static Exception TokenExpected(Token token, string id)
        {
            return new EntitySqlException(() => Res.TokenExpected, token, id);
        }

        public static Exception TokenExpected(Token token, Keyword id)
        {
            return new EntitySqlException(() => Res.TokenExpected, token, id.ToString().ToUpper());
        }


        public static Exception Token1OrToken2Expected(Token token, string id1, string id2)
        {
            return new EntitySqlException(() => Res.Token1OrToken2Expected, token, id1, id2);
        }


        public static Exception Token1OrToken2Expected(Token token, Keyword id1, Keyword id2)
        {
            return new EntitySqlException(() => Res.Token1OrToken2Expected, token,
                                          id1.ToString().ToUpper(), id2.ToString().ToUpper());
        }

        public static Exception CannotResolveNameToType(Token token, string typeName)
        {
            return new EntitySqlException(() => Res.CannotResolveNameToType, token, typeName);
        }

        public static Exception KeywordNotSupported(Token token, string keyword)
        {
            return new EntitySqlException(() => Res.ADP_KeywordNotSupported, token, keyword);
        }

        public static Exception KeywordExpected(TokenPosition errorPos, Keyword keyword)
        {
            Debug.Assert(keyword != Keyword.None);

            var k = keyword.ToString().ToUpper();
            return new EntitySqlException(() => Res.KeywordExpected, errorPos, k);
        }

        public static Exception TypeNameExpected(Token token)
        {
            return new EntitySqlException(() => Res.TypeNameExpected, token);
        }

        public static Exception UnknownPropertyOrField(Token token, string propertyOrFieldName, string type)
        {
            return new EntitySqlException(() => Res.UnknownPropertyOrField, token, propertyOrFieldName, type);
        }

        public static Exception ArgumentTypesAreIncompatible(TokenPosition errorPos, Type type1, Type type2)
        {
            return new EntitySqlException(() => Res.ArgumentTypesAreIncompatible, errorPos, type1.Name, type2.Name);
        }

        public static Exception NoCanonicalAggrFunctionOverloadMatch(Token errorToken,
                                                                     string methodName, IEnumerable<Type> types)
        {
            var typeNames = types.Select(o => o.Name).ToArray();
            var methodDefine = string.Format("{0}({1})", methodName, string.Join(",", typeNames));
            return new EntitySqlException(() => Res.NoCanonicalAggrFunctionOverloadMatch, errorToken,
                                          methodName, methodDefine);
        }

        public static Exception NoCanonicalFunctionOverloadMatch(Token errorToken, string methodName, IEnumerable<Type> argumentTypes)
        {
            var typeNames = argumentTypes.Select(o => o.Name).ToArray();
            var methodDefine = string.Format("{0}({1})", methodName, string.Join(",", typeNames));
            return new EntitySqlException(() => Res.NoCanonicalFunctionOverloadMatch, errorToken,
                                         methodName, methodDefine);
        }

        public static Exception MissingAsClause(Token errorToken)
        {
            return new EntitySqlException(() => Res.MissingAsClause, errorToken);
        }

        public static Exception NoPublicPropertyOrField(Token errorToken, string propName, Type objectType)
        {
            return new EntitySqlException(() => Res.NoPublicPropertyOrField, errorToken, propName, objectType.Name);
        }

        public static Exception NoPublicPropertyOrField(string propName, Type objectType)
        {
            return new EntitySqlException(() => Res.NoPublicPropertyOrField, propName, objectType.Name);
        }

        public static Exception CanNotFindConstructor(Token errorToken, Type objectType, IEnumerable<Type> argTypes)
        {
            var args = string.Join(",", argTypes.Select(o => o.Name).ToArray());
            return new EntitySqlException(() => Res.CanNotFindConstructor, errorToken, objectType.Name, args);
        }

        public static Exception CanNotFindMethod(Token errorToken, Type declareType, string methodName, IEnumerable<Type> argTypes)
        {
            var args = string.Join(",", argTypes.Select(o => o.Name).ToArray());
            return new EntitySqlException(() => Res.CanNotFindMethod, errorToken, methodName, args, declareType.Name);
        }

        public static Exception NotMethodOrType(Token errorToken, Type declareType, string methodName, IEnumerable<Type> argTypes)
        {
            var args = string.Join(",", argTypes.Select(o => o.Name).ToArray());
            return new EntitySqlException(() => Res.NotMethodOrType, errorToken, methodName, args, declareType.Name);

        }

        public static Exception HavingRequiresGroupClause(Token errorToken)
        {
            return new EntitySqlException(() => Res.HavingRequiresGroupClause, errorToken);
        }

        public static Exception NoMatchingMethod(Token errorToken, string methodName, IEnumerable<Type> argTypes)
        {
            var args = string.Join(",", argTypes.Select(o => o.Name).ToArray());
            var methodDefine = string.Format("{0}({1})", methodName, args);
            return new EntitySqlException(() => Res.NoMatchingMethod, errorToken, methodDefine);
        }

        public static Exception RequirePublicPropertySetter(Token errorToken, PropertyInfo property)
        {
            var typeName = property.DeclaringType.Name;
            var propertyName = property.Name;
            return new EntitySqlException(() => Res.RequirePublicPropertySetter, errorToken, typeName, propertyName);
        }

        public static Exception PropertyNoSetter(Token errorToken, PropertyInfo property)
        {
            var typeName = property.DeclaringType.Name;
            var propertyName = property.Name;
            return new EntitySqlException(() => Res.PropertyNoSetter, errorToken, typeName, propertyName);
        }

        public static Exception RequirePublicField(Token errorToken, FieldInfo field)
        {
            var typeName = field.DeclaringType.Name;
            var fieldName = field.Name;
            return new EntitySqlException(() => Res.RequirePublicField, errorToken, typeName, fieldName);
        }


        public static Exception ParameterNameRequried(Token errorToken)
        {
            return new EntitySqlException(() => Res.ParameterNameRequried, errorToken);
        }

        public static Exception InvalidParameterType(string typeName)
        {
            return new EntitySqlException(() => Res.ObjectParameter_InvalidParameterType, typeName);
        }

        public static Exception AmbiguousTypeReference(string typeName, string ns1, string ns2)
        {
            return new EntitySqlException(() => Res.AmbiguousTypeReference, typeName, ns1, ns2);
        }

        public static Exception TopAndLimitCannotCoexist(Token errorToken)
        {
            return new EntitySqlException(() => Res.TopAndLimitCannotCoexist, errorToken);
        }

        public static Exception TopAndTakeCannotCoexist(Token errorToken)
        {
            return new EntitySqlException(() => Res.TopAndTakeCannotCoexist, errorToken);
        }

        public static Exception TopAndSkipCannotCoexist(Token errorToken)
        {
            return new EntitySqlException(() => Res.TopAndSkipCannotCoexist, errorToken);
        }

        public static Exception LimitIntegerRequired(Token errorToken)
        {
            return new EntitySqlException(() => Res.Cqt_Limit_IntegerRequired, errorToken);
        }

        public static Exception LimitConstantOrParameterRequired(Token errorToken)
        {
            return new EntitySqlException(() => Res.Cqt_Limit_ConstantOrParameterRefRequired, errorToken);
        }

        public static Exception LimitNonNegativeLimitRequired(Token errorToken)
        {
            return new EntitySqlException(() => Res.Cqt_Limit_NonNegativeLimitRequired, errorToken);
        }

        public static Exception LikeArgMustBeConstantOrParameter(Token errorToken)
        {
            return new EntitySqlException(() => Res.LikeArgMustBeConstantOrParameter, errorToken);
        }

        public static Exception LikeArgMustBeStringType(Token errorToken)
        {
            return new EntitySqlException(() => Res.LikeArgMustBeStringType, errorToken);
        }

        public static Exception LikeArgMustBeNotEmpty(Token errorToken)
        {
            return new EntitySqlException(() => Res.LikeArgMustBeNotEmpty, errorToken);
        }


        public static Exception ObjectParameterCollection_DuplicateParameterName(string parameterName)
        {
            return new EntitySqlException(() => Res.ObjectParameterCollection_DuplicateParameterName, parameterName);
        }

        public static Exception InvalidQueryCast(Type castSourceType, Type castTargetType)
        {
            Debug.Assert(castSourceType != null);
            Debug.Assert(castTargetType != null);

            return new EntitySqlException(() => Res.InvalidQueryCast, castSourceType.FullName, castTargetType.FullName);
        }


    }

    internal class EntitySqlException : Exception
    {
        public EntitySqlException(Expression<Func<string>> msgExpr, Token token, params object[] args)
            : this(msgExpr, args)
        {
            var pos = token.Position;
            this.ErrorContext = string.Format(Res.ErrorContextWithTerm, token.Text, pos.Line + 1, pos.Column + 1);
        }

        public EntitySqlException(Expression<Func<string>> msgExpr, TokenPosition pos, params object[] args)
            : this(msgExpr, args)
        {
            this.ErrorContext = string.Format(Res.ErrorContextWithoutPredicate, pos.Line + 1, pos.Column + 1);
        }

        public EntitySqlException(Expression<Func<string>> msgExpr, params object[] args)
        {
            var memberExpr = (MemberExpression)msgExpr.Body;
            var member = (PropertyInfo)memberExpr.Member;
            this.ErrorName = member.Name;
            var msg = (string)member.GetValue(null, null);

            this.ErrorDescription = string.Format(msg, args);

        }

        public EntitySqlException(string msgExpr, Token token, params object[] args)
        {
            var msg = msgExpr;
            var pos = token.Position;
            this.ErrorContext = string.Format(Res.ErrorContextWithTerm, token.Text, pos.Line + 1, pos.Column + 1);
            this.ErrorDescription = string.Format(msg, args);

        }

        public string ErrorName { get; set; }

        public string ErrorContext { get; private set; }

        public string ErrorDescription { get; private set; }

        public override string Message
        {
            get { return ErrorDescription + " " + ErrorContext; }
        }
    }
}
