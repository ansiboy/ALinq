using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.SqlClient;
using System.Reflection;
using System.Collections;

namespace ALinq
{
    /// <summary>
    /// Provides for immediate loading and filtering of related data.
    /// </summary>
    public sealed class DataLoadOptions
    {
        // Fields
        private bool frozen;
        private readonly Dictionary<MetaPosition, MemberInfo> includes = new Dictionary<MetaPosition, MemberInfo>();
        private readonly Dictionary<MetaPosition, LambdaExpression> subqueries = new Dictionary<MetaPosition, LambdaExpression>();

        /// <summary>
        /// Initializes a new instance of the ALinq.DataLoadOptions class.
        /// </summary>
        public DataLoadOptions()
        {
            
        }
        
        /// <summary>
        /// Filters objects retrieved for a particular relationship.
        /// </summary>
        /// <typeparam name="T">The type that is queried against.  If the type is unmapped, an exception is thrown.</typeparam>
        /// <param name="expression">Identifies the query to be used on a particular one-to-many field or property. Note the following: If the expression does not start with a field or property that represents a one-to-many relationship, an exception is thrown.  If an operator other than a valid operator appears in the expression, an exception is thrown. Valid operators are as follows: Where OrderBy ThenBy OrderByDescending ThenByDescending Take</param>
        public void AssociateWith<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            this.AssociateWithInternal(expression);
        }

        /// <summary>
        /// Filters the objects retrieved for a particular relationship.
        /// </summary>
        /// <param name="expression">Identifies the query to be used on a particular one-to-many field or property. Note the following: If the expression does not start with a field or property that represents a one-to-many relationship, an exception is thrown.  If an operator other than a valid operator appears in the expression, an exception is thrown. Valid operators are as follows: Where OrderBy ThenBy OrderByDescending ThenByDescending Take</param>
        public void AssociateWith(LambdaExpression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            AssociateWithInternal(expression);
        }

        private void AssociateWithInternal(LambdaExpression expression)
        {
            Expression body = expression.Body;
            while ((body.NodeType == ExpressionType.Convert) || (body.NodeType == ExpressionType.ConvertChecked))
            {
                body = ((UnaryExpression)body).Operand;
            }
            LambdaExpression lambda = Expression.Lambda(body, expression.Parameters.ToArray<ParameterExpression>());
            System.Reflection.MemberInfo association = Searcher.MemberInfoOf(lambda);
            Subquery(association, lambda);
        }

        internal void Freeze()
        {
            frozen = true;
        }

        internal LambdaExpression GetAssociationSubquery(MemberInfo member)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }
            LambdaExpression expression = null;
            subqueries.TryGetValue(new MetaPosition(member), out expression);
            return expression;
        }

        private static Type GetIncludeTarget(MemberInfo mi)
        {
            Type memberType = TypeSystem.GetMemberType(mi);
            if (memberType.IsGenericType)
            {
                return memberType.GetGenericArguments()[0];
            }
            return memberType;
        }

        private static MemberInfo GetLoadWithMemberInfo(LambdaExpression lambda)
        {
            Expression body = lambda.Body;
            if ((body != null) && ((body.NodeType == ExpressionType.Convert) || (body.NodeType == ExpressionType.ConvertChecked)))
            {
                body = ((UnaryExpression)body).Operand;
            }
            var expression2 = body as MemberExpression;
            if ((expression2 == null) || (expression2.Expression.NodeType != ExpressionType.Parameter))
            {
                throw Error.InvalidLoadOptionsLoadMemberSpecification();
            }
            return expression2.Member;
        }

        internal bool IsPreloaded(MemberInfo member)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }
            return includes.ContainsKey(new MetaPosition(member));
        }

        /// <summary>
        /// Specifies which sub-objects to retrieve when a query is submitted for an object of type T.
        /// </summary>
        /// <typeparam name="T">Type that is queried against.  If this type is unmapped, an exception is thrown.</typeparam>
        /// <param name="expression">Identifies the field or property to be retrieved.  If the expression does not identify a field or property that represents a one-to-one or one-to-many relationship, an exception is thrown.</param>
        public void LoadWith<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            MemberInfo loadWithMemberInfo = GetLoadWithMemberInfo(expression);
            Preload(loadWithMemberInfo);
        }

        /// <summary>
        /// Retrieves specified data related to the main target by using a lambda expression.
        /// </summary>
        /// <param name="expression">A lambda expression that identifies the related material.</param>
        public void LoadWith(LambdaExpression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            MemberInfo loadWithMemberInfo = GetLoadWithMemberInfo(expression);
            Preload(loadWithMemberInfo);
        }

        internal void Preload(MemberInfo association)
        {
            if (association == null)
            {
                throw Error.ArgumentNull("association");
            }
            if (frozen)
            {
                throw Error.IncludeNotAllowedAfterFreeze();
            }
            includes.Add(new MetaPosition(association), association);
            ValidateTypeGraphAcyclic();
        }

        private void Subquery(MemberInfo association, LambdaExpression subquery)
        {
            if (frozen)
            {
                throw Error.SubqueryNotAllowedAfterFreeze();
            }
            subquery = (LambdaExpression)Funcletizer.Funcletize(subquery);
            ValidateSubqueryMember(association);
            ValidateSubqueryExpression(subquery);
            this.subqueries[new MetaPosition(association)] = subquery;
        }

        private static void ValidateSubqueryExpression(LambdaExpression subquery)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(subquery.Body.Type))
            {
                throw Error.SubqueryMustBeSequence();
            }
            new SubqueryValidator().VisitLambda(subquery);
        }

        private static void ValidateSubqueryMember(MemberInfo mi)
        {
            Type memberType = TypeSystem.GetMemberType(mi);
            if (memberType == null)
            {
                throw Error.SubqueryNotSupportedOn(mi);
            }
            if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(memberType))
            {
                throw Error.SubqueryNotSupportedOnType(mi.Name, mi.DeclaringType);
            }
        }

        private void ValidateTypeGraphAcyclic()
        {
            IEnumerable<MemberInfo> values = this.includes.Values;
            int num = 0;
            for (int i = 0; i < this.includes.Count; i++)
            {
                var source = new HashSet<Type>();
                foreach (MemberInfo info in values)
                {
                    source.Add(GetIncludeTarget(info));
                }
                var list = new List<MemberInfo>();
                bool flag = false;
                using (IEnumerator<MemberInfo> enumerator2 = values.GetEnumerator())
                {
                    Func<Type, bool> predicate = null;
                    while (enumerator2.MoveNext())
                    {
                        MemberInfo edge = enumerator2.Current;
                        if (predicate == null)
                        {
                            predicate = delegate(Type et)
                            {
                                if (!et.IsAssignableFrom(edge.DeclaringType))
                                {
                                    return edge.DeclaringType.IsAssignableFrom(et);
                                }
                                return true;
                            };
                        }
                        if (source.Where<Type>(predicate).Any<Type>())
                        {
                            list.Add(edge);
                        }
                        else
                        {
                            num++;
                            flag = true;
                            if (num == this.includes.Count)
                            {
                                return;
                            }
                        }
                    }
                }
                if (!flag)
                {
                    throw Error.IncludeCycleNotAllowed();
                }
                values = list;
            }
            throw new InvalidOperationException("Bug in ValidateTypeGraphAcyclic");
        }

        // Properties
        internal bool IsEmpty
        {
            get
            {
                return ((this.includes.Count == 0) && (this.subqueries.Count == 0));
            }
        }

        // Nested Types
        private static class Searcher
        {
            // Methods
            internal static MemberInfo MemberInfoOf(LambdaExpression lambda)
            {
                var visitor = new Visitor();
                visitor.VisitLambda(lambda);
                return visitor.MemberInfo;
            }

            // Nested Types
            private class Visitor : ALinq.SqlClient.ExpressionVisitor
            {
                // Fields
                internal MemberInfo MemberInfo;

                // Methods
                public override Expression VisitMemberAccess(MemberExpression m)
                {
                    this.MemberInfo = m.Member;
                    return base.VisitMemberAccess(m);
                }

                public override Expression VisitMethodCall(MethodCallExpression m)
                {
                    this.Visit(m.Object);
                    foreach (Expression expression in m.Arguments)
                    {
                        this.Visit(expression);
                        return m;
                    }
                    return m;
                }
            }
        }

        private class SubqueryValidator : ALinq.SqlClient.ExpressionVisitor
        {
            // Fields
            private bool isTopLevel = true;

            // Methods
            public override Expression VisitMethodCall(MethodCallExpression m)
            {
                Expression expression;
                bool isTopLevel = this.isTopLevel;
                try
                {
                    if (this.isTopLevel && !SubqueryRules.IsSupportedTopLevelMethod(m.Method))
                    {
                        throw Error.SubqueryDoesNotSupportOperator(m.Method.Name);
                    }
                    this.isTopLevel = false;
                    expression = base.VisitMethodCall(m);
                }
                finally
                {
                    this.isTopLevel = isTopLevel;
                }
                return expression;
            }
        }
    }
}
