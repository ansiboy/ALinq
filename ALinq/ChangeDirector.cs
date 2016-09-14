using System.Collections.Generic;
using ALinq;
using ALinq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ALinq.Provider;

namespace ALinq
{
    internal abstract class ChangeDirector
    {
        //protected static Type DataManipulationType = ReflectObject.GetType("ALinq.Provider.DataManipulation");
        // Methods
        protected ChangeDirector()
        {
        }

        internal abstract void AppendDeleteText(TrackedObject item, StringBuilder appendTo);
        internal abstract void AppendInsertText(TrackedObject item, StringBuilder appendTo);
        internal abstract void AppendUpdateText(TrackedObject item, StringBuilder appendTo);
        internal static ChangeDirector CreateChangeDirector(DataContext context)
        {
            return new StandardChangeDirector(context);
        }

        public abstract int Delete(TrackedObject item);
        public abstract int DynamicDelete(TrackedObject item);
        public abstract int DynamicInsert(TrackedObject item);
        public abstract int DynamicUpdate(TrackedObject item);
        public abstract int Insert(TrackedObject item);
        public abstract int Update(TrackedObject item);

        // Nested Types
        internal class StandardChangeDirector : ChangeDirector
        {
            // Fields
            private readonly DataContext context;

            // Methods
            public StandardChangeDirector(DataContext context)
            {
                this.context = context;
            }

            internal override void AppendDeleteText(TrackedObject item, StringBuilder appendTo)
            {
                if (item.Type.Table.DeleteMethod != null)
                {
                    appendTo.Append(Strings.DeleteCallbackComment);
                }
                else
                {
                    Expression deleteCommand = GetDeleteCommand(item);
                    appendTo.Append(context.Provider.GetQueryText(deleteCommand));
                    appendTo.AppendLine();
                }
            }

            internal override void AppendInsertText(TrackedObject item, StringBuilder appendTo)
            {
                if (item.Type.Table.InsertMethod != null)
                {
                    appendTo.Append(Strings.InsertCallbackComment);
                }
                else
                {
                    Expression insertCommand = GetInsertCommand(item);
                    appendTo.Append(context.Provider.GetQueryText(insertCommand));
                    appendTo.AppendLine();
                }
            }

            internal override void AppendUpdateText(TrackedObject item, StringBuilder appendTo)
            {
                if (item.Type.Table.UpdateMethod != null)
                {
                    appendTo.Append(Strings.UpdateCallbackComment);
                }
                else
                {
                    Expression updateCommand = GetUpdateCommand(item);
                    appendTo.Append(context.Provider.GetQueryText(updateCommand));
                    appendTo.AppendLine();
                }
            }

            internal static void AutoSyncMembers(object[] syncResults, TrackedObject item, UpdateType updateType)
            {
                if (syncResults != null)
                {
                    int num = 0;
                    var members = GetAutoSyncMembers(item.Type, updateType);
                    foreach (var member in members)
                    {
                        var obj2 = syncResults[num++];
                        var current = item.Current;
                        if ((member.Member is PropertyInfo) && ((PropertyInfo)member.Member).CanWrite)
                        {
                            member.MemberAccessor.SetBoxedValue(ref current, DBConvert.ChangeType(obj2, member.Type));
                        }
                        else
                        {
                            member.StorageAccessor.SetBoxedValue(ref current, DBConvert.ChangeType(obj2, member.Type));
                        }
                    }
                }
            }

            private static Expression CreateAutoSync(ICollection<MetaDataMember> membersToSync, Expression source)
            {
                int num = 0;
                var initializers = new Expression[membersToSync.Count];
                foreach (MetaDataMember member in membersToSync)
                {
                    initializers[num++] = Expression.Convert(GetMemberExpression(source, member.Member), typeof(object));
                }
                return Expression.NewArrayInit(typeof(object), initializers);
            }

            public override int Delete(TrackedObject item)
            {
                if (item.Type.Table.DeleteMethod == null)
                {
                    return DynamicDelete(item);
                }
                try
                {
                    item.Type.Table.DeleteMethod.Invoke(context, new[] { item.Current });
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null)
                    {
                        throw exception.InnerException;
                    }
                    throw;
                }
                return 1;
            }

            public override int DynamicDelete(TrackedObject item)
            {
                Expression deleteCommand = GetDeleteCommand(item);
                var returnValue = (int)context.Provider.Execute(deleteCommand).ReturnValue;
                if (returnValue == 0)
                {
                    deleteCommand = GetDeleteVerificationCommand(item);
                    var nullable = (int?)context.Provider.Execute(deleteCommand).ReturnValue;
                    returnValue = nullable.HasValue ? nullable.GetValueOrDefault() : -1;
                }
                return returnValue;
            }

            public override int DynamicInsert(TrackedObject item)
            {
                Expression insertCommand = GetInsertCommand(item);
                if (insertCommand.Type == typeof(int))
                {
                    return (int)context.Provider.Execute(insertCommand).ReturnValue;
                }
                var result = context.Provider.Execute(insertCommand).ReturnValue;
                var returnValue = (IEnumerable<object>)result;
                object[] syncResults;
                var value = returnValue.FirstOrDefault();
                
                if (value is object[])
                    syncResults = (object[])value;
                else
                    syncResults = new[] { value };

                if (syncResults == null)
                {
                    throw Error.InsertAutoSyncFailure();
                }
                AutoSyncMembers(syncResults, item, UpdateType.Insert);
                return 1;
            }

            public override int DynamicUpdate(TrackedObject item)
            {
                Expression updateCommand = GetUpdateCommand(item);
                if (updateCommand.Type == typeof(int))
                {
                    return (int)context.Provider.Execute(updateCommand).ReturnValue;
                }
                var returnValue = (IEnumerable<object>)context.Provider.Execute(updateCommand).ReturnValue;
                var syncResults = (object[])returnValue.FirstOrDefault();
                if (syncResults != null)
                {
                    AutoSyncMembers(syncResults, item, UpdateType.Update);
                    return 1;
                }
                return 0;
            }

            internal static List<MetaDataMember> GetAutoSyncMembers(MetaType metaType, UpdateType updateType)
            {
                var list = new List<MetaDataMember>();
                foreach (MetaDataMember member in metaType.PersistentDataMembers.OrderBy(m => m.Ordinal))
                {
                    if ((((updateType != UpdateType.Insert) || (member.AutoSync != AutoSync.OnInsert)) && 
                         ((updateType != UpdateType.Update) || (member.AutoSync != AutoSync.OnUpdate))) && 
                        (member.AutoSync != AutoSync.Always))
                    {
                        continue;
                    }
                    list.Add(member);
                }
                return list;
            }

            private static Expression GetDeleteCommand(TrackedObject tracked)
            {
                MetaType type = tracked.Type;
                MetaType inheritanceRoot = type.InheritanceRoot;
                ParameterExpression expression = Expression.Parameter(inheritanceRoot.Type, "p");
                Expression serverItem = expression;
                if (type != inheritanceRoot)
                {
                    serverItem = Expression.Convert(expression, type.Type);
                }
                object obj2 = tracked.CreateDataCopy(tracked.Original);
                Expression updateCheck = GetUpdateCheck(serverItem, tracked);
                if (updateCheck != null)
                {
                    updateCheck = Expression.Lambda(updateCheck, new[] { expression });
                    return Expression.Call(typeof(DataManipulation), "Delete", new[] { inheritanceRoot.Type },
                                           new[] { Expression.Constant(obj2), updateCheck });
                }
                return Expression.Call(typeof(DataManipulation), "Delete", new[] { inheritanceRoot.Type },
                                       new Expression[] { Expression.Constant(obj2) });
            }

            private Expression GetDeleteVerificationCommand(TrackedObject tracked)
            {
                ITable table = context.GetTable(tracked.Type.InheritanceRoot.Type);
                ParameterExpression left = Expression.Parameter(table.ElementType, "p");
                Expression expression2 = Expression.Lambda(Expression.Equal(left, Expression.Constant(tracked.Current)), new[] { left });
                Expression expression3 = Expression.Call(typeof(Queryable), "Where", new[] { table.ElementType },
                                                         new[] { table.Expression, expression2 });
                Expression expression4 = Expression.Lambda(Expression.Constant(0, typeof(int?)), new[] { left });
                Expression expression5 = Expression.Call(typeof(Queryable), "Select",
                                                         new[] { table.ElementType, typeof(int?) }, new[] { expression3, expression4 });
                return Expression.Call(typeof(Queryable), "SingleOrDefault", new[] { typeof(int?) }, new[] { expression5 });
            }

            private static Expression GetInsertCommand(TrackedObject item)
            {
                var autoSyncMembers = GetAutoSyncMembers(item.Type, UpdateType.Insert);
                var source = Expression.Parameter(item.Type.Table.RowType.Type, "p");
                if (autoSyncMembers.Count > 0)
                {
                    LambdaExpression expression3 = Expression.Lambda(CreateAutoSync(autoSyncMembers, source), new[] { source });
                    return Expression.Call(typeof(DataManipulation), "Insert", new[] { item.Type.InheritanceRoot.Type, expression3.Body.Type }, new Expression[] { Expression.Constant(item.Current), expression3 });
                }
                return Expression.Call(typeof(DataManipulation), "Insert", new[] { item.Type.InheritanceRoot.Type }, new Expression[] { Expression.Constant(item.Current) });
            }

            private static Expression GetMemberExpression(Expression exp, MemberInfo mi)
            {
                var field = mi as FieldInfo;
                if (field != null)
                {
                    return Expression.Field(exp, field);
                }
                var property = (PropertyInfo)mi;
                return Expression.Property(exp, property);
            }

            private static Expression GetUpdateCheck(Expression serverItem, TrackedObject tracked)
            {
                MetaType type = tracked.Type;
                if (type.VersionMember != null)
                {
                    return Expression.Equal(GetMemberExpression(serverItem, type.VersionMember.Member), GetMemberExpression(Expression.Constant(tracked.Current), type.VersionMember.Member));
                }
                Expression left = null;
                foreach (MetaDataMember member in type.PersistentDataMembers)
                {
                    if (!member.IsPrimaryKey)
                    {
                        UpdateCheck updateCheck = member.UpdateCheck;
                        if ((updateCheck == UpdateCheck.Always) || ((updateCheck == UpdateCheck.WhenChanged) && tracked.HasChangedValue(member)))
                        {
                            object boxedValue = member.MemberAccessor.GetBoxedValue(tracked.Original);
                            Expression right = Expression.Equal(GetMemberExpression(serverItem, member.Member), Expression.Constant(boxedValue, member.Type));
                            left = (left != null) ? Expression.And(left, right) : right;
                        }
                    }
                }
                return left;
            }

            private Expression GetUpdateCommand(TrackedObject tracked)
            {
                object original = tracked.Original;
                MetaType inheritanceType = tracked.Type.GetInheritanceType(original.GetType());
                MetaType inheritanceRoot = inheritanceType.InheritanceRoot;
                ParameterExpression expression = Expression.Parameter(inheritanceRoot.Type, "p");
                Expression serverItem = expression;
                if (inheritanceType != inheritanceRoot)
                {
                    serverItem = Expression.Convert(expression, inheritanceType.Type);
                }
                Expression updateCheck = GetUpdateCheck(serverItem, tracked);
                if (updateCheck != null)
                {
                    updateCheck = Expression.Lambda(updateCheck, new[] { expression });
                }
                List<MetaDataMember> autoSyncMembers = GetAutoSyncMembers(inheritanceType, UpdateType.Update);
                if (autoSyncMembers.Count > 0)
                {
                    LambdaExpression lambdaExpression = Expression.Lambda(CreateAutoSync(autoSyncMembers, serverItem), new[] { expression });
                    if (updateCheck != null)
                    {
                        return Expression.Call(typeof(DataManipulation), "Update", new[] { inheritanceRoot.Type, lambdaExpression.Body.Type }, new[] { Expression.Constant(tracked.Current), updateCheck, lambdaExpression });
                    }
                    return Expression.Call(typeof(DataManipulation), "Update", new[] { inheritanceRoot.Type, lambdaExpression.Body.Type }, new Expression[] { Expression.Constant(tracked.Current), lambdaExpression });
                }
                if (updateCheck != null)
                {
                    return Expression.Call(typeof(DataManipulation), "Update", new[] { inheritanceRoot.Type }, new[] { Expression.Constant(tracked.Current), updateCheck });
                }
                return Expression.Call(typeof(DataManipulation), "Update", new[] { inheritanceRoot.Type }, new Expression[] { Expression.Constant(tracked.Current) });
            }

            public override int Insert(TrackedObject item)
            {
                if (item.Type.Table.InsertMethod == null)
                {
                    return this.DynamicInsert(item);
                }
                try
                {
                    item.Type.Table.InsertMethod.Invoke(this.context, new object[] { item.Current });
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null)
                    {
                        throw exception.InnerException;
                    }
                    throw;
                }
                return 1;
            }

            public override int Update(TrackedObject item)
            {
                if (item.Type.Table.UpdateMethod == null)
                {
                    return this.DynamicUpdate(item);
                }
                try
                {
                    item.Type.Table.UpdateMethod.Invoke(this.context, new object[] { item.Current });
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException != null)
                    {
                        throw exception.InnerException;
                    }
                    throw;
                }
                return 1;
            }

            // Nested Types
            internal enum UpdateType
            {
                Insert,
                Update,
                Delete
            }
        }
    }
}
