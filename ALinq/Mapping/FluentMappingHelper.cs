using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal static class FluentMappingHelper
    {
        public static EntityMapping<TEntity> Map<TContext, TEntity>(this DataContextMapping<TContext> dataContextMapping,
                                                                    Expression<Func<TContext, Table<TEntity>>> predicate)
            where TContext : DataContext
            where TEntity : class
        {
            var entityMapping = Map<TEntity>(dataContextMapping);
            return entityMapping;
        }

        public static EntityMapping<TEntity> Map<TContext, TEntity>(this DataContextMapping<TContext> dataContextMapping,
                                                            Expression<Func<TContext, Table<TEntity>>> predicate,
                                                            TableAttribute tableAttribute)
            where TContext : DataContext
            where TEntity : class
        {
            var entityMapping = Map<TEntity>(dataContextMapping);
            entityMapping.TableAttribute = tableAttribute;
            return entityMapping;
        }

        public static EntityMapping<TEntity> Map<TEntity>(this DataContextMapping dataContextMapping)
            where TEntity : class
        {
            var entityMapping = dataContextMapping.GetEntityMapping<TEntity>();
            if (entityMapping == null)
                entityMapping = new EntityMapping<TEntity>();

            dataContextMapping.Add(entityMapping);

            return entityMapping;
        }

        public static EntityMapping<TEntity> Map<TEntity>(this DataContextMapping dataContextMapping, TableAttribute tableAttribute) where TEntity : class
        {
            var entityMapping = Map<TEntity>(dataContextMapping);
            entityMapping.TableAttribute = tableAttribute;
            return entityMapping;
        }

        public static EntityMapping<TEntity> Table<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                                      TableAttribute tableAttribute)
            where TEntity : class
        {
            entityMapping.TableAttribute = tableAttribute;
            return entityMapping;
        }

        public static EntityMapping<TEntity> Table<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                            string tableName) where TEntity : class
        {
            TableAttribute tableAttribute;
            if (!string.IsNullOrEmpty(tableName))
                tableAttribute = new TableAttribute { Name = tableName };
            else
                tableAttribute = new TableAttribute();

            return Table(entityMapping, tableAttribute);
        }

        public static EntityMapping<TEntity> Table<TEntity>(this EntityMapping<TEntity> entityMapping) where TEntity : class
        {
            return Table(entityMapping, string.Empty);
        }

        public static EntityMapping<TEntity> Inheritance<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                                  InheritanceMappingAttribute inheritance) where TEntity : class
        {
            entityMapping.InheritanceMappingAttribute.Add(inheritance);
            return entityMapping;
        }



        public static EntityMapping<TEntity> Inheritance<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                          string code,
                                                          Type inheritanceType) where TEntity : class
        {
            return Inheritance(entityMapping, code, inheritanceType, false);
        }

        public static EntityMapping<TEntity> Inheritance<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                                  string code,
                                                                  Type inheritanceType,
                                                                  bool isDefault) where TEntity : class
        {
            var inheritance = new InheritanceMappingAttribute { Code = code, Type = inheritanceType, IsDefault = isDefault };
            entityMapping.InheritanceMappingAttribute.Add(inheritance);
            return entityMapping;
        }

        #region ColumAttribute Functions
        public static EntityMapping<TEntity> Column<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                 Expression<Func<TEntity, object>> member) where TEntity : class
        {
            return Column(entityMapping, member, new ColumnAttribute { UpdateCheck = UpdateCheck.Never });
        }

        public static EntityMapping<TEntity> Column<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                          Expression<Func<TEntity, object>> predicate,
                                                          ColumnAttribute column) where TEntity : class
        {
            var mi = GetMember(predicate);
            return Column(entityMapping, mi, column);
        }

        public static EntityMapping<TEntity> Column<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                          Expression<Func<TEntity, object>> predicate,
                                                          Func<ColumnAttribute, ColumnAttribute> func) where TEntity : class
        {
            var mi = GetMember(predicate);
            var column = new ColumnAttribute() { UpdateCheck = UpdateCheck.Never };
            column = func(column);
            return Column(entityMapping, mi, column);
        }

        public static EntityMapping<TEntity> Column<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                  MemberInfo member,
                                                  Func<ColumnAttribute, ColumnAttribute> func) where TEntity : class
        {
            var mi = member;
            var column = new ColumnAttribute() { UpdateCheck = UpdateCheck.Never };
            column = func(column);
            return Column(entityMapping, mi, column);
        }

        public static EntityMapping<TEntity> Column<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                  MemberInfo mi,
                                                  ColumnAttribute column) where TEntity : class
        {
            if (mi.DeclaringType != typeof(TEntity) && !typeof(TEntity).IsSubclassOf(mi.DeclaringType))
                throw Error.MappedMemberHadNoCorrespondingMemberInType(mi.Name, typeof(TEntity).Name);

            entityMapping.Add(mi, column);
            return entityMapping;
        }

        public static ColumnAttribute GetColumn<TEntity>(this EntityMapping<TEntity> entityMapping,
                                                         Expression<Func<TEntity, object>> predicate) where TEntity : class
        {
            var mi = GetMember(predicate);
            if (mi.DeclaringType != typeof(TEntity) && !typeof(TEntity).IsSubclassOf(mi.DeclaringType))
                throw Error.MappedMemberHadNoCorrespondingMemberInType(mi.Name, typeof(TEntity).Name);

            return entityMapping.GetColumn(mi);
        }
        #endregion

        #region FunctionAttribute Functions
        public static DataContextMapping<TContxt> Function<TContxt>(this DataContextMapping<TContxt> contextMapping,
                                                            Expression<Func<TContxt, object>> member) where TContxt : DataContext
        {
            return Function(contextMapping, member, new FunctionAttribute());
        }

        public static DataContextMapping<TContxt> Function<TContxt>(this DataContextMapping<TContxt> contextMapping,
                                                        Expression<Func<TContxt, object>> member, bool isComposable) where TContxt : DataContext
        {
            return Function(contextMapping, member, new FunctionAttribute() { IsComposable = isComposable });
        }

        public static DataContextMapping<TContxt> Function<TContxt>(this DataContextMapping<TContxt> contextMapping,
                                                            Expression<Func<TContxt, object>> member, bool isComposable, string name) where TContxt : DataContext
        {
            return Function(contextMapping, member, new FunctionAttribute() { IsComposable = isComposable, Name = name });
        }

        public static DataContextMapping<TContxt> Function<TContxt>(this DataContextMapping<TContxt> contextMapping,
                                                          Expression<Func<TContxt, object>> predicate,
                                                          FunctionAttribute function) where TContxt : DataContext
        {
            var mi = GetMember(predicate);
            return Function(contextMapping, mi, function);
        }

        public static DataContextMapping<TContext> Function<TContext>(this DataContextMapping<TContext> contextMapping,
                                                  Expression<Func<TContext, object>> predicate,
                                                  Func<FunctionAttribute, FunctionAttribute> func) where TContext : DataContext
        {
            var mi = GetMember(predicate);
            var function = new FunctionAttribute();
            function = func(function);
            return Function(contextMapping, mi, function);
        }

        static DataContextMapping<TContxt> Function<TContxt>(this DataContextMapping<TContxt> contextMapping,
                                                        MemberInfo mi,
                                                        FunctionAttribute function) where TContxt : DataContext
        {
            if (mi.DeclaringType != typeof(TContxt))
                throw Error.MappedMemberHadNoCorrespondingMemberInType(mi.Name, typeof(TContxt).Name);

            contextMapping.Add(mi, function);
            return contextMapping;
        }

        public static FunctionAttribute GetFunction<TContxt>(this DataContextMapping<TContxt> entityMapping,
                                                             Expression<Func<TContxt, object>> predicate) where TContxt : DataContext
        {
            var mi = GetMember(predicate);
            if (mi.DeclaringType != typeof(TContxt))
                throw Error.MappedMemberHadNoCorrespondingMemberInType(mi.Name, typeof(TContxt).Name);

            return entityMapping.GetFunction(mi);
        }
        #endregion

        //public static void Add<TContext>(this FluentMappingSource mappingSource,
        //                                 DataContextMapping<TContext> contextMapping) where TContext : DataContext
        //{
        //    mappingSource.Add(contextMapping);
        //}

        //public static DataContextMapping<TContext> GetDataContextMapping<TContext>(this FluentMappingSource mappingSource) where TContext : DataContext
        //{
        //    var result = mappingSource.GetDataContextMapping(typeof(TContext));
        //    return (DataContextMapping<TContext>)result;
        //}

        private static MemberInfo GetMember(Expression predicate)
        {
            var visitor = new Visitor();
            visitor.Visit(predicate);
            return visitor.Member;
        }

        public static EntityMapping<TEntity> Association<TEntity, TEntity1>(this EntityMapping<TEntity> entityMapping,
                                                          Expression<Func<TEntity, TEntity1>> predicate,
                                                          Func<AssociationAttribute, AssociationAttribute> func)
            where TEntity : class
            where TEntity1 : class
        {
            var mi = GetMember(predicate);
            if (mi.DeclaringType != typeof(TEntity) && !typeof(TEntity).IsSubclassOf(mi.DeclaringType))
                throw Error.MappedMemberHadNoCorrespondingMemberInType(mi.Name, typeof(TEntity).Name);

            var association = new AssociationAttribute();
            association = func(association);
            entityMapping.Add(mi, association);
            return entityMapping;
        }

        public static EntityMapping<TEntity> Association<TEntity, TEntity1>(this EntityMapping<TEntity> entityMapping,
                                                                  Expression<Func<TEntity, TEntity1>> predicate,
                                                                  AssociationAttribute association)
            where TEntity : class
            where TEntity1 : class
        {
            var mi = GetMember(predicate);
            if (mi.DeclaringType != typeof(TEntity) && !typeof(TEntity).IsSubclassOf(mi.DeclaringType))
                throw Error.MappedMemberHadNoCorrespondingMemberInType(mi.Name, typeof(TEntity).Name);

            entityMapping.Add(mi, association);
            return entityMapping;
        }

        public static ColumnAttribute DefaultAutoSync(this ColumnAttribute column)
        {
            column.AutoSync = Mapping.AutoSync.Default;
            return column;
        }

        public static ColumnAttribute AutoSyncOnInsert(this ColumnAttribute column)
        {
            column.AutoSync = Mapping.AutoSync.OnInsert;
            return column;
        }

        public static ColumnAttribute AutoSyncOnUpdate(this ColumnAttribute column)
        {
            column.AutoSync = Mapping.AutoSync.OnUpdate;
            return column;
        }

        public static ColumnAttribute AlwaysAutoSync(this ColumnAttribute column)
        {
            column.AutoSync = Mapping.AutoSync.Always;
            return column;
        }

        public static ColumnAttribute NeverAutoSync(this ColumnAttribute column)
        {
            column.AutoSync = Mapping.AutoSync.Never;
            return column;
        }

        public static ColumnAttribute NotNull(this ColumnAttribute column)
        {
            column.CanBeNull = false;
            return column;
        }

        public static ColumnAttribute Null(this ColumnAttribute column)
        {
            column.CanBeNull = true;
            return column;
        }

        public static ColumnAttribute DbType(this ColumnAttribute column, string value)
        {
            column.DbType = value;
            return column;
        }

        public static ColumnAttribute DbGenerated(this ColumnAttribute column)
        {
            column.IsDbGenerated = true;
            return column;
        }

        public static ColumnAttribute Discriminator(this ColumnAttribute column)
        {
            column.IsDiscriminator = true;
            return column;
        }

        public static ColumnAttribute Expression(this ColumnAttribute column, string value)
        {
            column.Expression = value;
            return column;
        }

        public static ColumnAttribute PrimaryKey(this ColumnAttribute column)
        {
            column.IsPrimaryKey = true;
            return column;
        }

        public static ColumnAttribute Version(this ColumnAttribute column)
        {
            column.IsVersion = true;
            return column;
        }

        public static ColumnAttribute AlwaysUpdateCheck(this ColumnAttribute column)
        {
            column.UpdateCheck = Mapping.UpdateCheck.Always;
            return column;
        }

        public static ColumnAttribute NeverUpdateCheck(this ColumnAttribute column)
        {
            column.UpdateCheck = Mapping.UpdateCheck.Never;
            return column;
        }

        public static ColumnAttribute UpdateCheckWhenChanged(this ColumnAttribute column)
        {
            column.UpdateCheck = Mapping.UpdateCheck.WhenChanged;
            return column;
        }

        public static T Name<T>(this T data, string value) where T : DataAttribute
        {
            data.Name = value;
            return data;
        }

        public static T Storage<T>(this T data, string value) where T : DataAttribute
        {
            data.Storage = value;
            return data;
        }

        public static AssociationAttribute DeleteOnNull(this AssociationAttribute association)
        {
            association.DeleteOnNull = true;
            return association;
        }

        public static AssociationAttribute ForeignKey(this AssociationAttribute association)
        {
            association.IsForeignKey = true;
            return association;
        }

        public static AssociationAttribute Keys(this AssociationAttribute association, string thisKey, string otherKey)
        {
            association.ThisKey = thisKey;
            association.OtherKey = otherKey;
            return association;
        }

        public static FunctionAttribute Composable(this FunctionAttribute function)
        {
            function.IsComposable = true;
            return function;
        }

        public static FunctionAttribute Name(this FunctionAttribute function, string name)
        {
            function.Name = name;
            return function;
        }
        //public static InheritanceMappingAttribute Code(this InheritanceMappingAttribute inheritance, string code)
        //{
        //    inheritance.Code = code;
        //    return inheritance;
        //}

        //public static DataContextMapping Provider(this DataContextMapping contextMapping, Type providerType)
        //{
        //    var attr = new ProviderAttribute(providerType);
        //    contextMapping.ProviderAttribute = attr;
        //    return contextMapping;
        //}

        //public static DatabaseAttribute Database(this DataContextMapping contextMapping, string databaseName)
        //{
        //    var attr = new DatabaseAttribute { Name = databaseName };
        //    contextMapping.DatabaseAttribute = attr;
        //    return attr;
        //}

        //public static DatabaseAttribute Database(this DataContextMapping contextMapping)
        //{
        //    var attr = new DatabaseAttribute();
        //    contextMapping.DatabaseAttribute = attr;
        //    return attr;
        //}

        class Visitor : ALinq.SqlClient.ExpressionVisitor
        {
            private MemberInfo member;
            private Dictionary<Expression, Type> targetTypes = new Dictionary<Expression, Type>();

            public MemberInfo Member
            {
                get { return member; }
            }

            public override Expression VisitMemberAccess(MemberExpression node)
            {
                if (this.member == null)
                {
                    this.member = node.Member;
                    if (member.DeclaringType != node.Expression.Type &&
                       node.Expression.Type.IsSubclassOf(member.DeclaringType))
                    {
                        var bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                 BindingFlags.GetProperty | BindingFlags.GetField;
                        member = node.Expression.Type.GetMember(member.Name, bf)[0];
                    }
                    return node;
                }
                return base.VisitMemberAccess(node);
            }

            public override System.Linq.Expressions.Expression VisitUnary(UnaryExpression u)
            {
                targetTypes[u.Operand] = u.Type;
                return base.VisitUnary(u);
            }

            //Eval
            class NameEvalVisitor :  ALinq.SqlClient.ExpressionVisitor
            {
                public override Expression VisitMemberAccess(MemberExpression m)
                {
                    var member = m.Member;
                    Visit(m.Expression);
                    if (Result != null)
                    {
                        if (member is FieldInfo)
                        {
                            var obj = ((FieldInfo)member).GetValue(Result);
                            Result = obj;
                        }
                        else if (member is PropertyInfo)
                        {
                            var obj = ((PropertyInfo)member).GetValue(Result, null);
                            Result = obj;
                        }
                        else if (member is MethodInfo)
                        {
                            var obj = ((MethodInfo)member).Invoke(Result, new object[] { });
                            Result = obj;
                        }

                    }
                    //else if(member is MethodInfo)
                    //{
                    //    var obj = ((MethodInfo) member).Invoke(Result, null);
                    //}
                    //return base.VisitMemberAccess(m);
                    return m;
                }

                public override System.Linq.Expressions.Expression VisitConstant(ConstantExpression c)
                {
                    Result = c.Value;
                    return c;
                }

                private object _result;
                public object Result
                {
                    get { return _result; }
                    private set { _result = value; }
                }
            }

            public override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (this.member == null)
                {
                    if (m.Method.Name == "get_Item" && m.Arguments.Count == 1)
                    {
                        var visitor = new NameEvalVisitor();
                        visitor.Visit(m.Arguments[0]);
                        string name = visitor.Result as string;
                        if (name == null)
                        {
                            throw new Exception(string.Format("Can not evaluate expression {0}.", m.Arguments[0]));
                        }

                        Type type;
                        if (!targetTypes.TryGetValue(m, out type))
                            type = m.Method.ReturnType;

                        if (type.IsValueType && !TypeSystem.IsNullableType(type))
                            type = typeof(Nullable<>).MakeGenericType(type);

                        var mf = new IndexerMemberInfo(m.Method, name, type);
                        member = mf;
                    }
                    else
                    {
                        member = m.Method;
                    }
                    return m;
                }
                return base.VisitMethodCall(m);
            }

        }
    }
}