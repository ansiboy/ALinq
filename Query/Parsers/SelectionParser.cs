using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using E = ALinq.Dynamic.ExpressionUtility;
#if L2S
using ITable = System.Data.Linq.ITable;
#endif

namespace ALinq.Dynamic.Parsers
{
    partial class QueryParser
    {
        class SelectionParser : IParser
        {

            private TokenCursor tokenCursor;
            private QueryParser parser;
            private Expression source;
            private ParameterExpression it;
            private Type expectedElementType;

            public SelectionParser(QueryParser parser, Expression source, ParameterExpression it)
                : this(parser, source, it, null)
            {

            }

            public SelectionParser(QueryParser parser, Expression source, ParameterExpression it, Type expectedElementType)
            {
                Debug.Assert(parser != null);

                this.parser = parser;
                this.source = source;
                this.tokenCursor = parser.TokenCursor;
                this.it = it;
                this.expectedElementType = expectedElementType;
            }

            Token Token
            {
                get { return tokenCursor.Current; }
            }

            void NextToken()
            {
                this.tokenCursor.NextToken();
            }

            bool IsAggregateFunction(Function function)
            {
                return function == Function.BigCount || function == Function.Count || function == Function.Max ||
                       function == Function.Min || function == Function.Sum || function == Function.Average;
            }

            public Expression ParseSelect()
            {
                //=====================================================
                // 例句：
                // select EmployeeID, row (FirstName as F, LastName as L) as M) 
                // from Employees
                // 在一开始调用时，将形如 EmployeeID, xxxx 作为 row 来解释，在解释
                // row(FirstName as F, LastName as L)时，则作为单个的表达式来解释。
                //=====================================================
                ParameterExpression p = it;

                Expression expr;
                switch (Token.Keyword)
                {
                    case Keyword.Row:
                    case Keyword.Value:
                        expr = ParseSelection();
                        break;
                    case Keyword.None:
                        expr = this.ParseRow(p, source);
                        break;
                    default:
                        throw Error.GenericSyntaxError(Token);
                }

                return expr;
            }

            Expression IParser.ParseExpression()
            {
                Expression expr;
                switch (Token.Keyword)
                {
                    case Keyword.Row:
                    case Keyword.Value:
                        expr = ParseSelection();
                        break;
                    case Keyword.None:
                        expr = this.parser.ParseExpression();
                        break;
                    case Keyword.GroupPartition:
                        expr = ParseGroupPartition();
                        break;
                    case Keyword.Select:
                        expr = this.parser.ParseExpression();
                        break;
                    default:
                        throw Error.GenericSyntaxError(Token);
                }

                return expr;
            }



            private Expression ParseGroupPartition()
            {
                NextToken();
                var errorToken = Token;
                var args = this.ParseArgumentList();
                if (args.Length != 1)
                    throw Error.GenericSyntaxError(errorToken);

                var partitionParameter = ExpressionUtility.FindParameter(args[0]);
                Debug.Assert(partitionParameter != null);
                var lambda = Expression.Lambda(args[0], partitionParameter);
                ParameterExpression p = it;
                var partition = ExpressionUtility.Call("Select", new[] { partitionParameter.Type, args[0].Type },
                                                p, lambda);

                return partition;
            }

            TokenCursor IParser.TokenCursor
            {
                get { return this.tokenCursor; }
            }


            private Expression ParseSelection()
            {
                var query = this.source;

                ParameterExpression p = it;

                Expression expr;
                switch (Token.Keyword)
                {
                    case Keyword.Value:
                        bool isGroupQuery = ExpressionUtility.IsGroupQuery(query);
                        NextToken();


                        if (IsAggregateFunction(Token.Function))
                        {
                            bool isSubQuery;
                            var agg = this.ParseAggregate(p, query, out isSubQuery);
                            expr = agg;
                            if (!isSubQuery)
                                return agg;
                        }
                        else
                        {
                            expr = ((IParser)this).ParseExpression();
                            if (expr == null)
                                expr = ParseSelectionIdentifier();

                            if (expr == null)
                                return null;

                            if (isGroupQuery && expr.NodeType == ExpressionType.MemberAccess)
                            {
                                expr = this.parser.TranslateInnerGroupKey(expr, Token);
                            }

                            //==========================================================
                            // 例句：
                            // select value row(p.CategoryId, p.UnitPrice) from Products as p 
                            // group by p.CategoryId, p.UnitPrice
                            // having max(p.UnitPrice) > 1000
                            // ---------------------------------------------------------
                            // 如果解釋生成的 Expression 已經是一個非字符串對象，進接返回即可
                            if (typeof(IQueryable).IsAssignableFrom(expr.Type))
                            {
                                var t = ExpressionUtility.ElementType(expr);
                                if (!t.IsValueType && t != typeof(string))
                                    return expr;
                            }
                            //==========================================================
                        }

                        Debug.Assert(p != null);

                        var elementType = ExpressionUtility.ElementType(query);
                        var lambda = Expression.Lambda(expr, new[] { p });

                        query = E.GenerateSelect(new[] { query, lambda }, new[] { elementType, lambda.Body.Type });

                        if (this.Token.Identity != TokenId.End && Token.Keyword == Keyword.None)
                            throw Error.InvalidSelectValueList(Token);

                        break;

                    case Keyword.Row:
                    case Keyword.None:
                        query = this.ParseRow(p, query);
                        break;
                    default:
                        throw Error.GenericSyntaxError(Token);
                }

                return query;

            }

            private Expression ParseSelectionIdentifier()
            {
                var className = Token.Text;
                NextToken();
                while (Token.Identity == TokenId.Dot)
                {
                    NextToken();
                    className = className + "." + Token.Text;
                    NextToken();
                }

                IEnumerable<string> namespaces = null;
                namespaces = this.parser.referenceNamespaces;
                var objectType = new TypeFinder().FindType(className, namespaces);
                if (objectType == null)
                    return null;

                if (Token.Identity != TokenId.OpenParen && Token.Identity != TokenId.OpenCurlyBrace)
                    throw Error.Token1OrToken2Expected(Token, "(", "{");

                Expression[] args = new Expression[] { };
                if (Token.Identity == TokenId.OpenParen)
                    args = this.ParseArgumentList();

                var cons = ExpressionUtility.FindConstructor(objectType, args);
                if (cons == null)
                    throw Error.CanNotFindConstructor(Token, objectType, args.Select(o => o.Type).ToArray());

                var newExpr = Expression.New(cons, args);
                if (Token.Identity == TokenId.OpenCurlyBrace)
                {
                    var expr = this.ParseMemberInit(newExpr);
                    return expr;
                }

                return newExpr;
            }

            #region Parse Aggregate Function
            private MethodCallExpression ParseAggregate(ParameterExpression it, Expression query, out bool isSubQuery)
            {
                var isGroupQuery = E.IsGroupQuery(query);
                Expression source = isGroupQuery ? it : query;

                Debug.Assert(this.IsAggregateFunction(Token.Function));
                var errorToken = Token;
                var function = Token.Function;
                NextToken();
                var args = this.ParseArgumentList();
                var methodName = function == Function.BigCount ? "LongCount" : function.ToString();

                if (function == Function.Count || function == Function.BigCount)
                {
                    if (args.Length == 0)
                    {
                        //===================================================================
                        // 解释如下例句：
                        // select value count() from products as p
                        //===================================================================

                        var t = E.ElementType(source);
                        var exp = E.GenerateMethodCall(function, new[] { source }, new[] { t });
                        isSubQuery = false;
                        return exp;
                    }

                    if (args.Length == 1)
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(args[0].Type) && args[0].Type != typeof(string))
                        {
                            //===================================================================
                            // 解释如下例句：
                            // 1、select value count(c.Products) from category as c
                            // 2、select c.CategoryId, count(select p from c.Products as p) as ProductsCount from category as c
                            // 注：只解释 count(c.Products)，count(select p from c.Products as p) 部份
                            //===================================================================

                            var t = E.ElementType(args[0]);
                            var exp = E.GenerateMethodCall(function, new[] { args[0] }, new[] { t });

                            isSubQuery = true;
                            return exp;
                        }
                        else
                        {
                            if (isGroupQuery)
                            {
                                //===================================================================
                                // 解释如下例句：
                                // 2、select value count(p.CategoryId) from products as p group by p.CategoryId
                                //===================================================================

                                Debug.Assert(args[0].NodeType == ExpressionType.MemberAccess);

                                var m = (MemberExpression)args[0];
                                var t = ExpressionUtility.ElementType(it);

                                var exp = ExpressionUtility.GenerateMethodCall(function, new[] { it }, new Type[] { t });
                                isSubQuery = true;
                                return exp;
                            }
                            else
                            {
                                //===================================================================
                                // 解释如下例句：
                                // 1、select value count(p.CategoryId) from products as p
                                //===================================================================

                                Debug.Assert(args[0].NodeType == ExpressionType.MemberAccess);

                                var m = (MemberExpression)args[0];
                                var t = ExpressionUtility.ElementType(query);

                                var exp = ExpressionUtility.GenerateMethodCall(function, new[] { query }, new Type[] { t });
                                isSubQuery = false;
                                return exp;
                            }

                        }
                    }

                    throw Error.NoCanonicalAggrFunctionOverloadMatch(errorToken, methodName, args.Select(o => o.Type));
                }

                if (function == Function.Min || function == Function.Max)
                {
                    if (args.Length == 1)
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(args[0].Type) && args[0].Type != typeof(string))
                        {
                            //=====================================================================
                            // 解释如下例句：
                            // select min(select value p.UnitPrice from c.Products as p) from Categories as c
                            // 注：只解释 min(select value p.UnitPrice from c.Products as p) 
                            //=====================================================================
                            var t = E.ElementType(args[0]);
                            var exp = E.GenerateMethodCall(function, new[] { args[0] }, new[] { t });
                            isSubQuery = true;
                            return exp;
                        }
                        else
                        {
                            if (isGroupQuery)
                            {
                                //===================================================================
                                // 解释如下例句：
                                // select value min(p.UnitPrice) from Products as p group by p.CategoryId
                                // 注：只解释 min(p.UnitPrice) 部份
                                //===================================================================

                                var t = E.ElementType(source);
                                var p = E.FindParameter(args[0]);
                                Debug.Assert(p != null);
                                var l = Expression.Lambda(args[0], p);

                                var exp = E.GenerateMethodCall(function, new[] { source, l }, new[] { t, args[0].Type });
                                isSubQuery = true;
                                return exp;
                            }
                            else
                            {

                                //===================================================================
                                // 解释如下例句：
                                // select value min(p.UnitPrice) from Products as p
                                //===================================================================

                                var t = ExpressionUtility.ElementType(source);
                                var p = E.FindParameter(args[0]);
                                Debug.Assert(p != null);
                                var l = Expression.Lambda(args[0], p);

                                var exp = E.GenerateMethodCall(function, new[] { source, l }, new[] { t, args[0].Type });

                                isSubQuery = false;
                                return exp;
                            }
                        }


                    }

                    throw Error.NoCanonicalAggrFunctionOverloadMatch(errorToken, methodName, args.Select(o => o.Type));
                }

                if (function == Function.Sum || function == Function.Average)
                {
                    if (args.Length == 1)
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(args[0].Type) && args[0].Type != typeof(string))
                        {
                            //===================================================================
                            // 解释如下例句：
                            // select sum(select value p.UnitPrice from c.Products as p) from Categories as c
                            // 注：只解释 sum(select value p.UnitPrice from c.Products as p)
                            //===================================================================

                            var t = E.ElementType(args[0]);
                            Debug.Assert(t.IsValueType);

                            var exp = E.GenerateMethodCall(function, new[] { args[0] }, new Type[0]);
                            isSubQuery = true;
                            return exp;
                        }
                        else
                        {
                            if (isGroupQuery)
                            {
                                //===================================================================
                                // 解释如下例句：
                                // select key, sum(o.Quantity) as SumQuantity  
                                // from OrderDetails as o group by o.Product as key
                                // 注：只解释 sum(o.Quantity) 
                                //===================================================================

                                var t = E.ElementType(source);
                                var p = E.FindParameter(args[0]);
                                Debug.Assert(p != null);

                                E.CheckAndPromoteArgument(typeof(IEnumerableSignatures), function, ref args[0], errorToken);
                                var l = Expression.Lambda(args[0], p);
                                var exp = E.GenerateMethodCall(function, new[] { source, l }, new[] { t });

                                isSubQuery = true;
                                return exp;
                            }
                            else
                            {
                                //===================================================================
                                // 解释如下例句：
                                // select sum(o.UnitPrice) from OrderDetails as o
                                //===================================================================

                                var t = E.ElementType(source);
                                var p = E.FindParameter(args[0]);
                                Debug.Assert(p != null);

                                E.CheckAndPromoteArgument(typeof(IEnumerableSignatures), function, ref args[0], errorToken);
                                var l = Expression.Lambda(args[0], p);
                                var exp = E.GenerateMethodCall(function, new[] { source, l }, new[] { t });

                                isSubQuery = false;
                                return exp;
                            }

                        }

                    }

                    throw Error.NoCanonicalAggrFunctionOverloadMatch(errorToken, methodName, args.Select(o => o.Type));
                }

                throw new NotSupportedException(function.ToString());
            }

            #endregion

            #region For ParseSelection Function
            private Expression ParseRow(ParameterExpression it, Expression query)
            {

                Expression expr;

                if (Token.Keyword == Keyword.Row)
                {

                    NextToken();

                    Token.Validate(TokenId.OpenParen, Res.OpenParenExpected);
                    NextToken();

                    expr = ParseObject(it, typeof(DataRow), query);

                    Token.Validate(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
                    NextToken();

                }
                else
                {
                    expr = ParseObject(it, typeof(DataRow), query);
                }

                return expr;
            }

            //1、語句：Select e.FirstName, e.LastName From Employees as e
            //將 e.FirstName, e.LastName 解釋成為表達式
            //2、語句：Select e From Employees as e
            //將 e 解釋成表達式
            private Expression ParseObject(ParameterExpression it, Type newObjectBaseType, Expression source)
            {
                var properties = new List<DynamicProperty>();
                List<Expression> expressions = new List<Expression>();

                var aggregateExpressions = new List<Expression>();
                bool isGroupQuery = false;
                if (source != null)
                    isGroupQuery = ExpressionUtility.IsGroupQuery(source);

                bool? aggregateFunctionIsSubQuery = null;
                Token errorToken;// = Token.Empty;
                while (true)
                {
                    var exprToken = errorToken = Token;
                    //errorToken = Token;

                    string propName = null;
                    Expression expr;
                    if (IsAggregateFunction(Token.Function))
                    {
                        bool isSubQuery;

                        expr = ParseAggregate(it, source, out isSubQuery);

                        aggregateFunctionIsSubQuery = isSubQuery;
                        aggregateExpressions.Add(expr);
                    }
                    else
                    {
                        expr = ((IParser)this).ParseExpression();
                        if (expr == null)
                            throw Error.UnknownIdentifier(Token);

                        if (isGroupQuery && expr.NodeType == ExpressionType.MemberAccess)
                            expr = parser.TranslateInnerGroupKey(expr, Token);

                        //=============================================================
                    }

                    //if (Token.Keyword == Keyword.As)
                    //{
                    //    NextToken();
                    //    propName = Token.GetIdentifier();
                    //    NextToken();
                    //}
                    //else
                    //{
                    propName = ParsePropertyName(expr, exprToken);
                    //}


                    expressions.Add(expr);
                    if (expressions.Count > 1)
                    {
                        if (aggregateExpressions.Count != expressions.Count //并非所有的 expression 都是聚合函数
                            && !isGroupQuery && aggregateFunctionIsSubQuery == false)
                        {
                            Debug.Assert(!(Equals(errorToken, Token.Empty)));
                            throw Error.InvalidGroupIdentifierReference(Token.Position, errorToken.Text);
                        }
                    }

                    properties.Add(new DynamicProperty(propName, expr.Type));
                    if (Token.Identity != TokenId.Comma)
                        break;

                    NextToken();


                }


                Type objectType = ClassFactory.Instance.GetDynamicClass(properties, newObjectBaseType);
                MemberBinding[] bindings = new MemberBinding[properties.Count];
                var num = 0;
                foreach (var property in properties)
                {
                    bindings[num] = Expression.Bind(objectType.GetProperty(property.Name), expressions[num]);
                    num = num + 1;
                }

                Expression memberInit = Expression.MemberInit(Expression.New(objectType), bindings);

                //非分组的查询，Query Selection 只有聚合函数。
                if (aggregateExpressions.Count == expressions.Count)
                {
                    if (aggregateFunctionIsSubQuery == false)
                    {
                        return memberInit;
                    }
                }


                if (source == null)
                    return memberInit;

                var p = it;
                Debug.Assert(p != null);

                //=================================================
                //如果只有一项，并且兼容期待的元素类型，则直接返回。
                if (properties.Count == 1)
                {
                    if (expectedElementType == null)
                        expectedElementType = ExpressionUtility.ElementType(source);

                    var property = properties.Single();
                    if (TypeUtility.IsCompatibleWith(property.Type, expectedElementType)) 
                        memberInit = expressions[0];

                }
                //==================================================

                var lambda = Expression.Lambda(memberInit, new[] { p });
                var query = ExpressionUtility.GenerateQueryMethod("Select", new[] { source, lambda }, lambda.Body.Type);
                return query;
            }

            private string UniquePropertyName(string propName)
            {
                var num = 1;
                while (propertyNames.Contains(propName))
                {
                    propName = propName + num;
                    num = num + 1;
                }

                return propName;
            }

            private List<string> propertyNames = new List<string>();

            string ParsePropertyName(Expression expr, Token exprToken)
            {
                //Debug.Assert(expr != null);
                string propName = null;
                if (Token.Keyword == Keyword.As)
                {
                    NextToken();
                    propName = Token.GetIdentifier();
                    if (propertyNames.Contains(propName))
                        throw Error.AliasNameAlreadyUsed(Token, propName);

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
                            var call = ((MethodCallExpression)expr);
                            if (call.Method.Name == "get_Item" && call.Arguments.Count == 1 &&
                                call.Arguments[0].NodeType == ExpressionType.Constant)
                            {
                                propName = ((ConstantExpression)call.Arguments[0]).Value.ToString();
                            }
                            else
                                propName = call.Method.Name;

                            break;
                        case ExpressionType.Parameter:
                            propName = ((ParameterExpression)expr).Name;
                            break;
                    }
                    propName = UniquePropertyName(propName);
                }


                if (propName == null)
                    throw Error.ParseError(exprToken, Res.MissingAsClause);

                Debug.Assert(propertyNames.Contains(propName) == false);
                propertyNames.Add(propName);

                return propName;
            }
            #endregion
        }
    }

}
