using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#if L2S
using System.Data.Linq;
#endif

namespace ALinq.Dynamic.Parsers
{
    //TODO:1、Cross Join 支持
    /// <summary>
    /// 用來解釋查詢語句
    /// </summary>
    partial class QueryParser : ExpressionParser
    {
        private VariablesStore variableStore;

        private object dataContext;
        private string dataContextTypeName;

        private Expression topExpr;
        private Expression dataConextExpr;
        private Stack<Keyword> parseClauses;
        private IEnumerable<string> referenceNamespaces;

        public QueryParser(string query, params ObjectParameter[] parameters)
            : this(null, query, parameters)
        {

        }

        public QueryParser(object dataContext, string query)
            : this(dataContext, query, new ObjectParameter[0])
        {

        }

        public QueryParser(object dataContext, string query, IEnumerable<ObjectParameter> parameters)
            : base(new TokenCursor(query), parameters)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentNullException("query");

            Expression.Constant(dataContext);
            this.dataContext = dataContext;


            this.variableStore = new VariablesStore();
            this.parseClauses = new Stack<Keyword>(10);
            this.GroupKeys = new Dictionary<MemberInfo, Expression>();
            this.referenceNamespaces = new List<string>();

            if (dataContext != null)
            {
                it = ExpressionUtility.CreateParameter(dataContext.GetType());
                this.dataConextExpr = Expression.Constant(dataContext);
                this.dataContextTypeName = dataContext.GetType().Name;
            }
        }

        private ParameterExpression it
        {
            get;
            set;
        }

        protected override object GetSymbol(string symbolName)
        {
            object value;
            if (this.variableStore.GetAvailableVariable(symbolName, out value))
                return value;

            if (symbolName == this.dataContextTypeName)
                return this.dataConextExpr;

            var result = base.GetSymbol(symbolName);
            return result;
        }

        public IQueryable Parse()
        {
            return this.Parse(null);
        }

        public IQueryable Parse(Type expectedElementType)
        {
            ParseNamespaceReference();

            var expr = this.ParseExpression();
            if (expr == null)
                throw Error.UnknownIdentifier(Token);

            Token.Validate(TokenId.End);

            var isSingleValue = !typeof(IEnumerable).IsAssignableFrom(expr.Type) || expr.Type == typeof(string);
            var provider = ExpressionUtility.FindQueryProvider(expr); //QueryProviderFinder.FindProvider(expr);
            if (provider == null || isSingleValue)
                provider = new ExpressionQueryProvider();
            else
                provider = new ProxyQueryProvider(provider);

            IQueryable query = provider.CreateQuery(expr);

            return query;

        }

        void ParseNamespaceReference()
        {
            var list = (List<string>)referenceNamespaces; 
            while (Token.Keyword == Keyword.Using)
            {
                NextToken();
                var ns = Token.GetIdentifier();
                Debug.Assert(!string.IsNullOrEmpty(ns));
                NextToken();

                while (Token.Identity == TokenId.Dot)
                {
                    NextToken();
                    ns = ns + "." + Token.GetIdentifier();
                    NextToken();
                }

                Token.Validate(TokenId.Semicolon, Res.SemicolonExpected);
                list.Add(ns);
            }

            if (list.Count > 0)
                NextToken();


            if (dataConextExpr != null && typeof(DataContext).IsAssignableFrom(dataConextExpr.Type))
                list.Add(this.dataContext.GetType().Namespace);

            return;
        }

        protected override IEnumerable<string> ReferenceNamespaces
        {
            get
            {
                return this.referenceNamespaces;
            }
        }

        protected override Type ParseType()
        {
            return this.ParseType(this.referenceNamespaces, true);
        }
        


        public Expression TranslateInnerGroupKey(Expression expr, Token token)
        {
            //===========================================
            // 判斷是否需要轉換
            var e = expr as MemberExpression;
            while (e != null)
            {
                // 已經 group key ，不需要轉換
                if (e.Member.Name == "Key" && e.Expression.Type.IsGenericType &&
                    e.Expression.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                {
                    return expr;
                }

                e = e.Expression as MemberExpression;

            }
            //===========================================
            var groupKeys = GroupKeys;
            Debug.Assert(groupKeys != null);

            var m = (MemberExpression)expr;
            Expression value;
            if (groupKeys.TryGetValue(m.Member, out value))
            {
                return value;
            }


            throw Error.InvalidGroupIdentifierReference(token.Position, m.Member.Name);

        }


        Type ElementType(Expression expr)
        {
            return ExpressionUtility.ElementType(expr);
        }


        private bool TokenIs(Keyword keyword)
        {
            return Token.Keyword == keyword;
        }

        void ParseParameter(Expression expr)
        {
            var errorToken = Token;
            Token.Validate(Keyword.As, Error.KeywordExpected);

            var elementType = ExpressionUtility.ElementType(expr);

            NextToken();
            Token.Validate(TokenId.Identifier);
            var parameter = ExpressionUtility.CreateParameter(elementType, Token.Text);
            NextToken();

            if (variableStore.LocalVariables.ContainsKey(parameter.Name))
                throw Error.AliasNameAlreadyUsed(errorToken, parameter.Name);

            variableStore.SetLocalVariable(parameter.Name, parameter);

            this.it = parameter;
        }


        Expression ParseSelect()
        {
            if (this.Token.Keyword != Keyword.Select)
                throw ParseError(Res.IdentifierExpected, Keyword.Select);



            var preIt = it;
            var pos = Token.Position;

            //==================================================================
            // 寻找该 Select 语句的 From 关键字。
            // 例句：
            // Select row((select count() from Products as p where p.CategoryId == c.CategoryId) as ProductCount
            //           CategoryName, CategoryId)
            // From Category as c
            // 从上面的语句可以看出，括号内的'from'关键都不是要找的。
            //-------------------------------------------------------------------
            var stack = new Stack();
            while (Token.Identity != TokenId.End)
            {
                NextToken();

                if (Token.Identity == TokenId.OpenParen)
                {
                    stack.Push(TokenId.OpenParen);
                }
                else if (Token.Identity == TokenId.CloseParen)
                {
                    if (stack.Count == 0)
                        throw ParseError(Res.GenericSyntaxError);//TODO:显示具体的异常信息

                    stack.Pop();
                }
                else if (Token.Keyword == Keyword.From)
                {
                    if (stack.Count == 0)
                        break;
                }
            }
            //===================================================================

            var query = ParseFrom();


            var current = Token.Position;
            this.TokenCursor.MoveTo(pos);

            Debug.Assert(this.Token.Keyword == Keyword.Select);
            NextToken();

            query = ParseSelection(query);
            Token.Validate(Keyword.From);

            this.TokenCursor.MoveTo(current);

            it = preIt;

            return query;
        }

        protected Expression ParseSelection(Expression query, ParameterExpression it)
        {
            this.it = it;
            return ParseSelection(query);
        }

        private Expression ParseSelection(Expression query)
        {

            parseClauses.Push(Keyword.Select);


            if (Token.Keyword == Keyword.Top)
            {
                NextToken();
                this.topExpr = base.ParseExpression();
            }



            var q = new SelectionParser(this, query, it).ParseSelect();

            parseClauses.Pop();//Pop Keyword.Select
            return q;
        }

        private Expression ParseFrom()
        {
            if (!TokenIs(Keyword.From))
                throw ParseError(Res.TokenExpected, Keyword.From);

            this.parseClauses.Push(Keyword.From);

            NextToken();

            var query = ParseSelectMany();
            if (query == null)
                throw Error.UnknownIdentifier(Token);

            parseClauses.Pop();

            if (this.Token.Keyword == Keyword.Where || this.Token.Keyword == Keyword.Having)
            {
                NextToken();
                query = ParseWhere(query, it);
            }

            if (Token.Keyword == Keyword.Group)
            {
                //Debug.Assert(TokenIs(Keyword.Group));
                NextToken();
                if (!TokenIs(Keyword.By))
                    throw ParseError(Res.TokenExpected, Keyword.By);

                NextToken();

                query = ParseGroupBy(query, it);
            }
            else if (Token.Keyword == Keyword.Order)
            {
                NextToken();
                if (!this.TokenIs(Keyword.By))
                    throw ParseError(Res.TokenExpected, Keyword.By);

                NextToken();
                query = ParseOrder(query);
            }

            return query;
        }

        #region GroupKeyReplacement
        class GroupKeyReplacement
        {
            public static Expression Execute(Expression expr, QueryParser variables, Token errorToken)
            {
                var visitor = new Visitor(variables, errorToken);
                expr = visitor.Visit(expr);
                return expr;
            }

            private class Visitor : ExpressionVisitor
            {
                private Token errorToken;
                private QueryParser parser;

                public Visitor(QueryParser parser, Token errorToken)
                {
                    this.parser = parser;
                    this.errorToken = errorToken;
                }

                protected override Expression VisitMemberAccess(MemberExpression m)
                {
                    var k = this.parser.TranslateInnerGroupKey(m, errorToken);
                    return k;
                }
            }
        }
        #endregion

        private Expression ParseHaving(Expression query, ParameterExpression it)
        {
            if (this.Token.Keyword != Keyword.Having)
                return query;

            if (ExpressionUtility.IsGroupQuery(query) == false)
                throw Error.HavingRequiresGroupClause(Token);

            NextToken();

            var expr = base.ParseExpression(); //new ConditionalParser(this).ParseExpression();
            if (expr.Type != typeof(bool))
                throw Error.ExpressionTypeMustBeBoolean(Token, Res.CtxWhereClause);

            expr = GroupKeyReplacement.Execute(expr, this, Token);

            var elementType = ExpressionUtility.ElementType(query);
            var p = it;
            var lambda = Expression.Lambda(expr, p);
            query = Expression.Call(typeof(Queryable), "Where", new[] { elementType }, query, lambda);

            return query;
        }

        protected Expression ParseGroupBy(Expression query, ParameterExpression it)
        {
            // ==========================================================================================================
            // 需要處理的異常
            // An aggregate named '{0}' cannot be used because the specified group keys include a key with the same name.
            // At least one group key or aggregate is required.
            // =========================================================================================================
            parseClauses.Push(Keyword.Group);

            var gropuKey = (MemberInitExpression)new SelectionParser(this, null, it).ParseSelect();

            var lamdba = Expression.Lambda(gropuKey, it);

            var elementType = ExpressionUtility.ElementType(query);
            var call = Expression.Call(typeof(Queryable), "GroupBy", new[] { elementType, gropuKey.Type }, query, lamdba);

            var p = ExpressionUtility.FindParameter(gropuKey);
            Debug.Assert(p != null);

            query = call;

            this.it = it = ExpressionUtility.CreateParameter(query, "");

            var groupSource = ExpressionUtility.FindParameter(gropuKey);
            Debug.Assert(groupSource != null);

            elementType = ElementType(query);
            var groupKeyMember = Expression.MakeMemberAccess(this.it, elementType.GetProperty("Key"));
            foreach (MemberAssignment b in gropuKey.Bindings)
            {
                var value = Expression.MakeMemberAccess(groupKeyMember, b.Member);
                this.variableStore.SetLocalVariable(b.Member.Name, value);
                GroupKeys[(((MemberExpression)b.Expression).Member)] = value;
            }

            query = ParseHaving(query, it);

            this.parseClauses.Pop();
            return query;
        }

        Dictionary<MemberInfo, Expression> GroupKeys
        {
            get;
            set;
        }

        private Expression ParsePaging(Expression query)
        {
            //===================================================================
            // 需要處理的異常
            // BetweenLimitsCannotBeUntypedNulls: The upper and lower limits of the BETWEEN expression cannot be untyped nulls.
            // BetweenLimitsTypesAreNotCompatible: The BETWEEN lower limit type '{0}' is not compatible with the upper limit type '{1}'.
            // BetweenLimitsTypesAreNotOrderComparable: The BETWEEN lower limit type '{0}' is not order-comparable with the upper limit type '{1}'.
            // BetweenValueIsNotOrderComparable: The BETWEEN value type '{0}' is not order-comparable with the limits common type '{1}'.
            //===================================================================
            if (Token.Keyword == Keyword.Skip)
            {
                if (this.topExpr != null)
                    throw Error.TopAndSkipCannotCoexist(Token);

                NextToken();
                query = ParseSkip(query);
            }

            if (Token.Keyword == Keyword.Take || Token.Keyword == Keyword.Limit)
            {
                if (this.topExpr != null && Token.Keyword == Keyword.Limit)
                    throw Error.TopAndLimitCannotCoexist(Token);

                if (this.topExpr != null && Token.Keyword == Keyword.Take)
                    throw Error.TopAndTakeCannotCoexist(Token);

                NextToken();
                query = ParseTake(query);
            }


            if (this.topExpr != null)
            {
                var elementType = ExpressionUtility.ElementType(query);
                query = Expression.Call(typeof(Queryable), "Take", new[] { elementType }, query, topExpr);
                this.topExpr = null;
            }
            return query;
        }

        protected Expression ParseSkip(Expression query, ParameterExpression it)
        {
            this.it = it;
            return ParseSkip(query);
        }

        private Expression ParseSkip(Expression query)
        {
            this.parseClauses.Push(Keyword.Skip);

            Expression expr = base.ParseExpression();
            query = Expression.Call(typeof(Queryable), "Skip", new[] { ElementType(query) }, query, expr);
            var elementType = ExpressionUtility.ElementType(query);
            it = ExpressionUtility.CreateParameter(elementType);

            this.parseClauses.Pop();
            return query;
        }

        protected Expression ParseTake(Expression query, ParameterExpression it)
        {
            this.it = it;
            return ParseTake(query);
        }

        private Expression ParseTake(Expression query)
        {
            this.parseClauses.Push(Keyword.Take);

            var errorToken = Token;
            var expr = base.ParseExpression();
            if (expr.Type != typeof(int))
                throw Error.LimitIntegerRequired(Token);

            if (expr.NodeType != ExpressionType.Constant)
                throw Error.LimitConstantOrParameterRequired(Token);

            var num = (int)((ConstantExpression)expr).Value;
            if (num < 0)
                throw Error.LimitNonNegativeLimitRequired(errorToken);

            var q = ExpressionUtility.GenerateQueryMethod("Take", new[] { query, expr });

            this.parseClauses.Pop();
            return q;
        }

        //===========================================================================
        // 構造如下的表達式來生成 LeftJoin 的 SQL 語句。
        // entity sql:
        // select o, d
        // from Orders as o
        //      left join OrderDetails as d on o.OrderID = d.OrderID
        //
        // lambda expression:
        // db.Orders.GroupJoin(db.OrderDetails, o => o.OrderID, o => o.OrderID, (o, x) => new { o, x })
        //          .SelectMany(t => t.x.DefaultIfEmpty(), (a, d) => new { a.o, d });
        //
        //-----------------------------------------------------------------------------
        private int xCount = 0;
        private Expression ParseLeftJoin(Expression query)
        {
            NextToken();
            var collection = this.ParseExpression();

            if (!this.TokenIs(Keyword.On))
                throw ParseError(Res.TokenExpected, Keyword.On);


            NextToken();
            //Expression.Convert()
            var filter = base.ParseExpression() as BinaryExpression;
            if (filter == null)
                throw ParseError(Res.GenericSyntaxError);

            var member1 = filter.Left;
            var member2 = filter.Right;

            ParameterExpression p1 = ExpressionUtility.FindParameter(member1);
            ParameterExpression p2 = ExpressionUtility.FindParameter(member2);

            var p3 = ExpressionUtility.CreateParameter(typeof(IEnumerable<>).MakeGenericType(p2.Type), "<>x" + (xCount++));

            var exprNew = CreateFlatObjectExpression(p1, p3);

            var types = new[] { ElementType(query), ElementType(collection), member1.Type, exprNew.Type };
            var exprs = new Expression[]
                            {
                                query, collection, Expression.Lambda(member1, p1),
                                Expression.Lambda(member2, p2), Expression.Lambda(exprNew, p1, p3)
                            };

            query = Expression.Call(typeof(Queryable), "GroupJoin", types, exprs);


            //创建调用 SelectMany 函数的第一个参数，形如 e => e.Orders
            var elementType = ElementType(query);
            var sourceParameter = ExpressionUtility.CreateParameter(elementType, "<>t");
            collection = Expression.MakeMemberAccess(sourceParameter, elementType.GetProperty(p3.Name));
            var collectionElementType = ElementType(collection);
            Type delegateType = typeof(Func<,>).MakeGenericType(ElementType(query), typeof(IEnumerable<>).MakeGenericType(collectionElementType));
            var expr = Expression.Call(typeof(Enumerable), "DefaultIfEmpty", new[] { collectionElementType }, collection);
            LambdaExpression arg1 = Expression.Lambda(delegateType, expr, sourceParameter);

            //创建调用 SelectMany 函数的第二个参数，形如 (a, b) => new { a, b }

            exprNew = CreateFlatObjectExpression(sourceParameter, p2);
            var resultSelector = Expression.Lambda(exprNew, new[] { sourceParameter, p2 });

            elementType = ExpressionUtility.ElementType(query);
            types = new[] { elementType, collectionElementType, resultSelector.Body.Type };
            query = Expression.Call(typeof(Queryable), "SelectMany", types, query, arg1, resultSelector);

            elementType = ExpressionUtility.ElementType(query);
            it = ExpressionUtility.CreateParameter(elementType);

            foreach (var p in it.Type.GetProperties())
            {
                var ma = Expression.MakeMemberAccess(it, p);
                variableStore.AppendLocalVariable(p.Name, ma, true);
            }

            return query;
        }

        private Expression ParseInnerJoin(Expression source)
        {
            Debug.Assert(it != null);
            var outerParameter = it;

            NextToken();

            var inner = this.ParseSourceExpression();
            Debug.Assert(inner != null);


            var innerParameter = it;

            if (!TokenIs(Keyword.On))
                throw ParseError(Res.GenericSyntaxError);

            NextToken();
            var joinOn = base.ParseExpression() as BinaryExpression;
            if (joinOn == null || joinOn.NodeType != ExpressionType.Equal)
                throw ParseError(Res.GenericSyntaxError);


            var outerKey = joinOn.Left as MemberExpression;
            var innerKey = joinOn.Right as MemberExpression;

            //TODO:為空則抛异常
            Debug.Assert(outerKey != null);
            Debug.Assert(innerKey != null);

            if (outerKey.Type != innerKey.Type)
            {
                throw Error.MemberTypeMismatch(innerKey.Member, outerKey.Member);
            }

            var outerKeySelectorType = typeof(Func<,>).MakeGenericType(ElementType(source), outerKey.Type);
            var outerKeySelector = Expression.Lambda(outerKeySelectorType, outerKey, outerParameter);

            var innerKeySelectorType = typeof(Func<,>).MakeGenericType(ElementType(inner), innerKey.Type);
            var innerKeySelector = Expression.Lambda(innerKeySelectorType, innerKey, innerParameter);

            var exprNew = CreateFlatObjectExpression(outerParameter, innerParameter);



            var resultSelector = Expression.Lambda(exprNew, new[] { outerParameter, innerParameter });

            var types = new[] { ElementType(source), ElementType(inner), outerKey.Type, resultSelector.Body.Type };
            var args = new[] { source, inner, outerKeySelector, innerKeySelector, resultSelector };
            var q = Expression.Call(typeof(Queryable), "Join", types, args);

            it = ExpressionUtility.CreateParameter(q);

            foreach (var p in it.Type.GetProperties())
            {
                var ma = Expression.MakeMemberAccess(it, p);
                variableStore.AppendLocalVariable(p.Name, ma, true);
            }

            return q;
        }

        #region CreateFlatObjectExpression
        //将两个类中的属性，合并到一个类去.第一个可能是动态类，第二个为非动态类
        //动态类1：{ a, b }，类2：{ c }
        //合并后：{ a, b, c }
        //即：(a, c) => new { a.a, a.b, c }
        Expression CreateFlatObjectExpression(ParameterExpression parameter1, ParameterExpression parameter2)
        {
            Debug.Assert(!IsDynamicType(parameter2.Type));

            Type objectType;
            MemberBinding[] bindings;
            var properties = new List<DynamicProperty>();
            if (IsDynamicType(parameter1.Type))
            {
                foreach (var p in parameter1.Type.GetProperties())
                    properties.Add(new DynamicProperty(p.Name, p.PropertyType));

                var paramter2Property = new DynamicProperty(parameter2.Name, parameter2.Type);
                properties.Add(paramter2Property);
                objectType = ClassFactory.Instance.GetDynamicClass(properties, typeof(DataRow));

                bindings = new MemberBinding[properties.Count];
                var num = 0;
                foreach (var p in properties)
                {
                    Expression ma;
                    if (p == paramter2Property)
                        ma = parameter2;
                    else
                        ma = Expression.MakeMemberAccess(parameter1, parameter1.Type.GetProperty(p.Name));

                    bindings[num++] = Expression.Bind(objectType.GetProperty(p.Name), ma);
                }
            }
            else
            {
                properties.Add(new DynamicProperty(parameter1.Name, parameter1.Type));
                properties.Add(new DynamicProperty(parameter2.Name, parameter2.Type));
                objectType = ClassFactory.Instance.GetDynamicClass(properties, typeof(DataRow));

                bindings = new MemberBinding[properties.Count];
                bindings[0] = Expression.Bind(objectType.GetProperty(parameter1.Name), parameter1);
                bindings[1] = Expression.Bind(objectType.GetProperty(parameter2.Name), parameter2);
            }



            var expr = Expression.MemberInit(Expression.New(objectType), bindings);
            return expr;
        }
        #endregion

        static bool IsDynamicType(Type type)
        {
            if (type.IsSubclassOf(typeof(DataRow)))//type.IsSubclassOf(typeof(DynamicObject)) ||
                return true;

            return false;
        }


        protected Exception ParseError(string format, params object[] args)
        {
            return ParseError(Token.Position, format, args);
        }

        Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
        }

        protected Expression ParseOrder(Expression query, ParameterExpression it)
        {
            this.it = it;
            return ParseOrder(query);
        }

        private Expression ParseOrder(Expression query)
        {

            var count = 0;
            while (true)
            {
                var expr = base.ParseExpression() as MemberExpression;
                if (expr == null)
                    throw ParseError(Res.GenericSyntaxError);//TODO:異常信息

                Debug.Assert(it != null);
                var lambda = Expression.Lambda(expr, it);
                MethodCallExpression call;


                if (Token.Keyword == Keyword.Asc)
                {
                    var methodName = count == 0 ? "OrderBy" : "ThenBy";
                    call = Expression.Call(typeof(Queryable), methodName, new[] { expr.Expression.Type, expr.Type }, query, lambda);
                    NextToken();
                }
                else if (Token.Keyword == Keyword.Desc)
                {
                    var methodName = count == 0 ? "OrderByDescending" : "ThenByDescending";
                    call = Expression.Call(typeof(Queryable), methodName, new[] { expr.Expression.Type, expr.Type }, query, lambda);
                    NextToken();
                }
                else if (Token.Identity == TokenId.Comma || Token.Identity == TokenId.End)
                {
                    var methodName = count == 0 ? "OrderBy" : "ThenBy";
                    call = Expression.Call(typeof(Queryable), methodName, new[] { it.Type, expr.Type }, query, lambda);
                }
                else if (Token.Keyword == Keyword.Skip || (Token.Keyword == Keyword.Limit || Token.Keyword == Keyword.Take))
                {
                    break;
                }
                else
                {
                    throw ParseError(Res.GenericSyntaxError);
                }

                count = count + 1;
                query = call;
                it = ExpressionUtility.CreateParameter(query);

                if (Token.Identity == TokenId.Comma)
                    NextToken();
                else
                    break;
            }

            return query;
        }

        protected Expression ParseWhere(Expression query, ParameterExpression it)
        {
            this.parseClauses.Push(Keyword.Where);
            var expr = base.ParseExpression();

            if (expr.Type != typeof(bool))
                throw Error.ExpressionTypeMustBeBoolean(Token, Res.CtxWhereClause);

            var elementType = ElementType(query);
            LambdaExpression lambda = Expression.Lambda(expr, it);
            query = ExpressionUtility.Call("Where", new[] { elementType }, query, lambda);

            this.parseClauses.Pop();
            return query;

        }

        Expression ParseCollectionOperation(Expression query)
        {
            switch (Token.Keyword)
            {
                case Keyword.Intersect:
                    return ParseIntersect(query);
                case Keyword.Except:
                    return ParseExcept(query);
                case Keyword.Union:
                    return ParseUnion(query);
            }

            return query;
        }

        Expression ParseExcept(Expression left)
        {
            if (left == null)
                left = ParseUnion(null);

            if (Token.Keyword == Keyword.Except)
            {
                NextToken();
                var right = ParseUnion(null);
                //var query = Expression.Call(typeof(System.Linq.Enumerable), "Except", new[] { ElementType(left) }, left, right);
                var query = ExpressionUtility.Call("Except", new[] { ElementType(left) }, left, right);
                return query;
            }
            return left;
        }

        Expression ParseUnion(Expression left)
        {
            if (left == null)
                left = ParseIntersect(null);

            if (Token.Keyword == Keyword.Union)
            {
                NextToken();
                var right = ParseIntersect(null);
                var query = ExpressionUtility.Call("Union", new[] { ElementType(left) }, left, right);
                return query;
            }
            return left;
        }

        Expression ParseIntersect(Expression left)
        {
            if (left == null)
                left = ParseExpression();

            if (Token.Keyword == Keyword.Intersect)
            {
                NextToken();
                var right = ParseExpression();
                var query = ExpressionUtility.Call("Intersect", new[] { ElementType(left) }, left, right);
                return query;
            }

            return left;
        }

        private Keyword? ParsingClause
        {
            get
            {
                if (this.parseClauses.Count == 0)
                    return null;

                return this.parseClauses.Peek();
            }
        }

        public override Expression ParseExpression()
        {
            return ParseExpression(null);
        }

        public Expression ParseExpression(Type expectedElementType)
        {
            Expression expr = null;
            switch (Token.Keyword)
            {
                case Keyword.Select:
                    {
                        expr = ParseSourceExpression();
                        return expr;
                    }
                case Keyword.Row:
                case Keyword.Value:
                    {
                        expr = new SelectionParser(this, expr, it, expectedElementType).ParseSelect();
                        return expr;
                    }
                default:
                    {
                        expr = base.ParseExpression();
                        Debug.Assert(expr != null);

                        expr = ParseCollectionOperation(expr);
                        return expr;
                    }
                //default:
                //    expr = base.ParseExpression();
                //    return expr;
            }

            throw Error.KeywordNotSupported(Token, Token.Keyword.ToString());

        }

        //=====================================================================================
        // SelectManay 例子:
        // var q1 = db.Employees.SelectMany(e => e.Orders, (a, b) => new { a, b })
        //                      .SelectMany(o => o.b.OrderDetails, (a, c) => new { a.a, a.b, c });
        //-------------------------------------------------------------------------------------    
        private Expression ParseSelectMany()
        {
            var left = ParseJoin();
            if (Token.Identity == TokenId.Comma)
            {
                NextToken();

                var preIt = this.it;
                var right = ParseJoin();

                //创建调用 SelectMany 函数的第一个参数，形如 e => e.Orders
                var collectionElementType = right.Type.GetGenericArguments()[0];
                //ParseParameter(right);
                var parameter = this.it;

                Type delegateType = typeof(Func<,>).MakeGenericType(ElementType(left), typeof(IEnumerable<>).MakeGenericType(collectionElementType));
                LambdaExpression arg1 = Expression.Lambda(delegateType, right, preIt);

                //创建调用 SelectMany 函数的第二个参数，形如 (a, b) => new { a, b }

                var exprNew = CreateFlatObjectExpression(preIt, it);
                var resultSelector = Expression.Lambda(exprNew, new[] { preIt, parameter });

                //调用 SelectMany 方法

                var types = new[] { ElementType(left), collectionElementType, resultSelector.Body.Type };
                var query = Expression.Call(typeof(Queryable), "SelectMany", types, left, arg1, resultSelector);

                it = ExpressionUtility.CreateParameter(query);

                //======================================================================================

                return query;
            }

            return left;
        }

        private Expression ParseJoin()
        {
            var query = ParseSourceExpression();
            return ParseJoin(query);
        }

        Expression ParseGroupPartition()
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
            var expr = Expression.Call(typeof(Enumerable), "Select", new[] { partitionParameter.Type, args[0].Type },
                                            p, lambda);
            ParseParameter(expr);
            return expr;
        }

        private Expression ParseSourceExpression()
        {
            Expression expr;
            if (Token.Identity == TokenId.OpenParen)
            {
                NextToken();
                expr = ParseSourceExpression();
                Token.Validate(TokenId.CloseParen);

                NextToken();
                if (Token.Keyword == Keyword.As)
                    ParseParameter(expr);

                return expr;
            }

            if (Token.Keyword == Keyword.Select)
            {
                variableStore.CreateLocalVariables();

                expr = ParseSelect();

                variableStore.ReleaseLocalVariables();
                expr = this.ParsePaging(expr);
                return expr;
            }

            if (Token.Keyword == Keyword.GroupPartition)
            {
                return ParseGroupPartition();
            }

            if (Token.Keyword == Keyword.None)
            {
                var errorToken = Token;
                if ((expr = base.ParseExpression()) == null)
                    throw Error.UnknownIdentifier(errorToken);


                if (Token.Keyword == Keyword.As)
                    ParseParameter(expr);

                return expr;
            }

            throw Error.KeywordNotSupported(Token, Token.Keyword.ToString());
        }

        private Expression ParseJoin(Expression query)
        {

            if (Token.Keyword == Keyword.Left)
            {
                NextToken();
                Token.Validate(Keyword.Join);

                query = ParseLeftJoin(query);
                query = ParseJoin(query);
            }
            else if (Token.Keyword == Keyword.Inner)
            {
                NextToken();
                Token.Validate(Keyword.Join);

                query = ParseInnerJoin(query);
                query = ParseJoin(query);
            }
            else if (Token.Keyword == Keyword.Join)
            {
                query = ParseInnerJoin(query);
                query = ParseJoin(query);
            }

            return query;
        }

        private Expression ParseTreat()
        {
            Debug.Assert(Token.Function == Function.Treat);
            NextToken();
            Token.Validate(TokenId.OpenParen);
            NextToken();

            var arg = ParseIdentifier();

            Token.Validate(Keyword.As, Error.KeywordExpected);
            NextToken();

            var targetType = this.ParseType(this.referenceNamespaces, true);
            var exp = Expression.Call(typeof(IQueryableUtility), "Treat", new Type[] { targetType }, arg);

            Token.Validate(TokenId.CloseParen);
            NextToken();

            return exp;
        }

        protected override Expression ParseIdentifier()
        {
            Expression expr;
            if ((expr = ParseFunction()) == null)
                expr = base.ParseIdentifier();

            if (expr != null)
                return expr;


            if (dataConextExpr != null && (expr = this.ParseMemberAccess(dataConextExpr)) != null)
            {
                if (this.ParsingClause == Keyword.From)
                {
                    var m = (MemberExpression)expr;
                    if (typeof(IQueryable).IsAssignableFrom(m.Type) == false)
                        throw Error.MemberMustBeIQueryable(Token, m.Member, dataConextExpr.Type);

                    if (Token.Keyword == Keyword.As)
                        ParseParameter(expr);
                }

                return expr;
            }

            return ParseMethodOrConstructor();
        }

        public Expression ParseMethodOrConstructor()
        {
            var errorToken = Token;
            Token methodToken = Token.Empty;

            string memberName = null;
            string typeName = Token.GetIdentifier();
            NextToken();

            string methodName = null;
            if (Token.Identity == TokenId.Dot)
            {
                NextToken();
                methodName = memberName = Token.Text;
                NextToken();
            }

            while (Token.Identity == TokenId.Dot)
            {
                Debug.Assert(memberName != null);

                NextToken();
                typeName = typeName + "." + memberName;
                methodName = memberName = Token.GetIdentifier();
                methodToken = Token;

                NextToken();
            }

            if (Token.Identity == TokenId.OpenParen || Token.Identity == TokenId.OpenCurlyBrace)
            {
                memberName = null;
            }
            else
            {
                methodName = null;
            }

            var typeFinder = new TypeFinder();
            var objectType = typeFinder.FindType(typeName, this.referenceNamespaces);
            if (objectType != null)
            {
                Expression expr;
                if (methodName != null)
                {
                    Expression[] args = new Expression[0];
                    MethodBase method;
                    ExpressionUtility.FindMethod(objectType, methodName, true, () => (args = this.ParseArgumentList()), out method); //objectType.GetMethod(methodName);
                    if (method == null)
                    {
                        Debug.Assert(methodToken.Equals(Token.Empty) == false);
                        throw Error.CanNotFindMethod(methodToken, objectType, methodName, args.Select(o => o.Type).ToArray());
                    }
                    //TODO:if method is not static then throw exception.
                    expr = Expression.Call(null, (MethodInfo)method, args);
                    return expr;
                }
                if (memberName != null)
                {
                    var member = TypeUtility.FindPropertyOrField(objectType, memberName, true);
                    if (member == null)
                        throw Error.NotPropertyOrField(memberName);

                    expr = Expression.MakeMemberAccess(null, member);
                    return expr;
                }

                if (Token.Identity == TokenId.OpenParen || Token.Identity == TokenId.OpenCurlyBrace)
                    expr = ParseConstructor(objectType);
                else
                    expr = ParseGetTableMethod(dataConextExpr, objectType);

                return expr;
            }

            typeName = typeName + "." + methodName;
            objectType = typeFinder.FindType(typeName, this.referenceNamespaces);
            if (objectType != null)
                return this.ParseConstructor(objectType);


            throw Error.UnknownIdentifier(errorToken);
        }

        private Expression ParseFunction()
        {
            Expression expr = null;
            var function = Token.Function;
            switch (function)
            {
                case Function.Average:
                case Function.BigCount:
                case Function.Count:
                case Function.Max:
                case Function.Min:
                case Function.Sum:
                    {
                        NextToken();
                        var args = this.ParseArgumentList();
                        expr = ExpressionUtility.GenerateAggregateExpression(function, it, args, Token);
                        break;
                    }
                case Function.Treat:
                    {
                        expr = ParseTreat();
                        break;
                    }
                case Function.OfType:
                    {
                        expr = ParseOfType();
                        break;
                    }
            }
            return expr;
        }

        private Expression ParseOfType()
        {
            NextToken();

            Token.Validate(TokenId.OpenParen, string.Format(Res.TokenExpected, "("));
            NextToken();

            var arg1 = this.ParseExpression();

            Token.Validate(TokenId.Comma, string.Format(Res.TokenExpected, ","));
            NextToken();

            bool isOnly = false;
            if (Token.Keyword == Keyword.Only)
            {
                isOnly = true;
                NextToken();
            }

            Token.Validate(TokenId.Identifier, Res.IdentifierExpected);

            //var op = Token;
            Type type = this.ParseType(this.referenceNamespaces, true);
            //if (type == null)
            //    throw Error.CannotResolveNameToType(Token, op.Text);

            Token.Validate(TokenId.CloseParen, string.Format(Res.TokenExpected, ")"));
            NextToken();

            var elementType = ExpressionUtility.ElementType(arg1);
            var p = ExpressionUtility.CreateParameter(elementType);
            Expression expr;
            if (!isOnly)
            {
                expr = Expression.TypeIs(p, type);
            }
            else
            {
                var method = elementType.GetMethod("GetType");
                var left = Expression.Call(p, method);
                var right = Expression.Constant(type);
                expr = Expression.MakeBinary(ExpressionType.Equal, left, right);
            }


            LambdaExpression lambda = null;
            lambda = Expression.Lambda(expr, p);
            var query = Expression.Call(typeof(Queryable), "Where", new[] { elementType }, arg1, lambda);

            return query;
        }

        Expression ParseConstructor(Type objectType)//, IEnumerable<string> namespaces)
        {
            //var tmpToken = Token;
            //var objectType = this.ParseType(namespaces); //new TypeFinder(namespaces).FindType(className);
            //if (objectType == null)
            //{
            //    this.TokenCursor.MoveTo(tmpToken.Position);
            //    return null;
            //}

            if (Token.Identity != TokenId.OpenParen && Token.Identity != TokenId.OpenCurlyBrace)
                throw Error.Token1OrToken2Expected(Token, "(", "{");

            Expression[] args = new Expression[0];
            MemberBinding[] members = new MemberBinding[0];

            if (Token.Identity == TokenId.OpenParen)
            {
                this.ParseArgumentList(objectType, out args, out members);
            }

            var cons = ExpressionUtility.FindConstructor(objectType, args);
            if (cons == null)
                throw Error.CanNotFindConstructor(Token, objectType, args.Select(o => o.Type).ToArray());

            var newExpr = Expression.New(cons, args);
            if (Token.Identity == TokenId.OpenCurlyBrace)
            {
                NextToken();

                var otherMembers = Token.Identity == TokenId.CloseCurlyBrace ? new MemberBinding[0] : this.ParseMemberBindings(objectType);
                if (otherMembers.Count() > 0)
                    members = members.Union(otherMembers).ToArray();

                Token.Validate(TokenId.CloseCurlyBrace, Error.TokenExpected(Token, "}"));
                NextToken();
            }

            if (members.Length > 0)
            {
                var expr = Expression.MemberInit(newExpr, members);
                return expr;
            }

            return newExpr;
            //}
        }

        private Expression ParseGetTableMethod(Expression instance, Type entityType)
        {
            Debug.Assert(instance != null);
            Debug.Assert(entityType != null);
            Debug.Assert(typeof(DataContext).IsAssignableFrom(instance.Type));


            var method = instance.Type.GetMethod("GetTable", new Type[0]);
            Debug.Assert(method != null);
            Debug.Assert(method.IsGenericMethod);

            method = method.MakeGenericMethod(entityType);
            var expr = Expression.Call(instance, method);
            return expr;
        }


        //For Case 语句
        Expression ParseWhenThen()
        {
            if (Token.Keyword == Keyword.When)
            {
                NextToken();
                var test = base.ParseExpression();
                if (Token.Keyword != Keyword.Then)
                    throw ParseError(Res.TokenExpected, Keyword.Then);

                var pos = Token.Position;
                NextToken();
                var then1 = this.ParseExpression();
                var then2 = this.ParseWhenThen();
                if (then2 == null)
                    then2 = then1.Type.IsValueType ? Expression.Constant(Convert.ChangeType(0, then1.Type), then1.Type) :
                                                     Expression.Constant(null, then1.Type);

                return ExpressionUtility.GenerateConditional(test, then1, then2, pos);

            }


            if (Token.Keyword == Keyword.Else)
            {
                NextToken();
                var e = this.ParseExpression();
                if (Token.Keyword != Keyword.End)
                    throw ParseError(Res.TokenExpected, Keyword.End);

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



        private Expression ParseExists()
        {
            Debug.Assert(Token.Keyword == Keyword.Exists);

            //var preIt = it;

            NextToken();
            Token.Validate(TokenId.OpenParen, Res.OpenParenExpected);

            NextToken();

            var query = this.ParseExpression();
            var expr = WhereFetch.GetFilter(query);

            if (expr == null)
                throw ParseError(Res.TokenExpected, Keyword.Where);

            var any = Expression.Call(typeof(Queryable), "Any", new[] { ElementType(query) }, query, expr);

            Token.Validate(TokenId.CloseParen, Res.CloseParenExpected);
            NextToken();

            //it = preIt;
            return any;
        }

        Expression ParseIn(Expression expr)
        {
            NextToken();
            var q = this.ParseExpression();
            if (typeof(IEnumerable).IsAssignableFrom(q.Type) == false)
                throw Error.InExpressionMustBeCollection(Token);

            var elementType = TypeUtility.GetElementType(q.Type);


            Debug.Assert(elementType != q.Type);

            var result = Expression.Call(typeof(Enumerable), "Contains", new[] { elementType }, q, expr);
            return result;
        }

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

        public void ParseArgumentList(Type objectType, out Expression[] args, out MemberBinding[] members)
        {
            Token.Validate(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();

            var argList = new List<Expression>();
            var memberList = new List<MemberBinding>();

            if (Token.Identity != TokenId.CloseParen)
            {
                while (true)
                {
                    var expr = ParseExpression();
                    if (expr == null)
                        throw Error.UnknownIdentifier(Token);

                    //var token = To;
                    if (Token.Keyword == Keyword.As)
                    {
                        NextToken();
                        var propName = Token.GetIdentifier();

                        var pf = TypeUtility.FindPropertyOrField(objectType, propName, false);
                        if (pf == null)
                            throw Error.NoPublicPropertyOrField(Token, propName, objectType);

                        if (pf.MemberType == MemberTypes.Property)
                        {
                            var p = ((PropertyInfo)pf);

                            if (p.CanWrite == false)
                                throw Error.PropertyNoSetter(Token, ((PropertyInfo)pf));

                            var setMethod = p.GetSetMethod();
                            if (setMethod == null || setMethod.IsPublic == false)
                                throw Error.RequirePublicPropertySetter(Token, p);
                        }
                        else
                        {
                            var f = (FieldInfo)pf;
                            if (f.IsPublic == false)
                                throw Error.RequirePublicField(Token, f);
                        }

                        var mb = Expression.Bind(pf, expr);
                        memberList.Add(mb);

                        NextToken();
                    }
                    else
                    {
                        argList.Add(expr);
                    }

                    if (Token.Identity != TokenId.Comma)
                        break;

                    NextToken();
                }
            }


            Token.Validate(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();

            args = argList.ToArray();
            members = memberList.ToArray();
        }

    }
}
