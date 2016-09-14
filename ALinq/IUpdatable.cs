#region MyRegion
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Services;
//using System.Diagnostics;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using ALinq.Mapping;
//using ALinq.SqlClient;

//namespace ALinq
//{
//    public partial class DataContext : IUpdatable
//    {
//        #region Subclass SqlUpdater
//        class SqlUpdater : IUpdatable
//        {
//            #region ExpressionVisitor
//            class ExpressionVisitor
//            {
//                private Dictionary<string, object> item;

//                public ExpressionVisitor(Dictionary<string, object> obj)
//                {
//                    this.item = obj;
//                }

//                public void Visit(Expression expression)
//                {
//                    switch (expression.NodeType)
//                    {
//                        case ExpressionType.Lambda:
//                            VisitLambdaExpression((LambdaExpression)expression);
//                            break;
//                        case ExpressionType.Call:
//                            VisitMethodCalExpression((MethodCallExpression)expression);
//                            break;
//                        case ExpressionType.Quote:
//                            VisitUnaryExpressionExpression((UnaryExpression)expression);
//                            break;
//                        case ExpressionType.Equal:
//                            VisitBinaryExpression((BinaryExpression)expression);
//                            break;
//                    }
//                }

//                private void VisitBinaryExpression(BinaryExpression expression)
//                {
//                    if (expression.Left is MemberExpression && expression.Right is ConstantExpression)
//                    {
//                        var memberExpression = (MemberExpression)expression.Left;
//                        var constantExpression = (ConstantExpression)expression.Right;

//                        var member = memberExpression.Member;
//                        var value = constantExpression.Value;
//                        item[member.Name] = value;
//                    }
//                }

//                private void VisitUnaryExpressionExpression(UnaryExpression expression)
//                {
//                    Visit(expression.Operand);
//                }

//                private void VisitMethodCalExpression(MethodCallExpression expression)
//                {
//                    foreach (var arg in expression.Arguments)
//                        Visit(arg);
//                }

//                private void VisitLambdaExpression(LambdaExpression expression)
//                {
//                    Visit(expression.Body);
//                }
//            }
//            #endregion

//            private readonly DataContext dataContext;
//            private readonly List<Dictionary<string, object>> items;
//            private const string TYPE_FIELD = "<>type";
//            private const string METHOD_FIELD = "<>method";

//            enum Methods
//            {
//                Insert,
//                Update,
//                Delete
//            }

//            public SqlUpdater(DataContext dataContext)
//            {
//                this.dataContext = dataContext;
//                items = new List<Dictionary<string, object>>();
//            }
//            /// <summary>
//            /// Creates the resource of the given type and belonging to the given container
//            /// </summary>
//            /// <param name="containerName">container name to which the resource needs to be added</param>
//            /// <param name="fullTypeName">full type name i.e. Namespace qualified type name of the resource</param>
//            /// <returns>object representing a resource of given type and belonging to the given container</returns>
//            object IUpdatable.CreateResource(string containerName, string fullTypeName)
//            {
//                var item = CreateItem();

//                item[TYPE_FIELD] = GetType(fullTypeName); //Type.GetType(fullTypeName, true);
//                item[METHOD_FIELD] = Methods.Insert;
//                return item;

//            }

//            Dictionary<string, object> CreateItem()
//            {
//                var item = new Dictionary<string, object>();
//                items.Add(item);
//                return item;
//            }

//            /// <summary>
//            /// Gets the resource of the given type that the query points to
//            /// </summary>
//            /// <param name="query">query pointing to a particular resource</param>
//            /// <param name="fullTypeName">full type name i.e. Namespace qualified type name of the resource</param>
//            /// <returns>object representing a resource of given type and as referenced by the query</returns>
//            object IUpdatable.GetResource(IQueryable query, string fullTypeName)
//            {
//                var item = CreateItem();
//                if (fullTypeName != null)
//                {
//                    item[TYPE_FIELD] = GetType(fullTypeName); //Type.GetType(fullTypeName, true);
//                    item[METHOD_FIELD] = Methods.Update;
//                }
//                else
//                {
//                    item[TYPE_FIELD] = query.ElementType;
//                    item[METHOD_FIELD] = Methods.Update;
//                }

//                SetMemberValue(item, query);

//                return item;
//            }

//            public void SetMemberValue(Dictionary<string, object> obj, IQueryable query)
//            {
//                var visitor = new ExpressionVisitor(obj);
//                visitor.Visit(query.Expression);
//            }

//            Type GetType(string fullTypeName)
//            {
//                //if (this.dataContext.Mapping is MappedMetaModel)
//                //{
//                //    string ns = null;
//                //    var strs = fullName.Split('.');
//                //    string name = strs[strs.Length - 1];

//                //    if (strs.Length > 1)
//                //        ns = string.Join(".", strs, 0, strs.Length - 1);

//                //    Type type = ns == null
//                //                    ? ((MappedMetaModel)this.dataContext.Mapping).FindType(name)
//                //                    : ((MappedMetaModel)this.dataContext.Mapping).FindType(name, ns);

//                //    return type;

//                //}
//                //else
//                //{
//                //    return Type.GetType(fullName, true);
//                //}

//                var type = Type.GetType(fullTypeName, false);
//                if (type != null)
//                    return type;

//                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
//                {
//                    foreach (Module module2 in assembly.GetLoadedModules())
//                    {
//                        type = module2.GetType(fullTypeName, false, false);
//                        if (type != null)
//                            return type;
//                    }
//                }

//                Debug.Assert(type == null);
//                throw new Exception(string.Format("Can not find type '{0}'.", fullTypeName));
//            }




//            /// <summary>
//            /// Resets the value of the given resource to its default value
//            /// </summary>
//            /// <param name="resource">resource whose value needs to be reset</param>
//            /// <returns>same resource with its value reset</returns>
//            object IUpdatable.ResetResource(object resource)
//            {
//                return resource;
//            }

//            /// <summary>
//            /// Sets the value of the given property on the target object
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="propertyValue">value of the property</param>
//            void IUpdatable.SetValue(object targetResource, string propertyName, object propertyValue)
//            {
//                var item = (Dictionary<string, object>)targetResource;
//                item[propertyName] = propertyValue;
//            }

//            /// <summary>
//            /// Gets the value of the given property on the target object
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <returns>the value of the property for the given target resource</returns>
//            object IUpdatable.GetValue(object targetResource, string propertyName)
//            {
//                var item = (Dictionary<string, object>)targetResource;
//                object value;
//                if (item.TryGetValue(propertyName, out value))
//                    return value;

//                return null;
//            }

//            /// <summary>
//            /// Sets the value of the given reference property on the target object
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="propertyValue">value of the property</param>
//            void IUpdatable.SetReference(object targetResource, string propertyName, object propertyValue)
//            {
//                //((IUpdatable)this).SetValue(targetResource, propertyName, propertyValue);
//                var type = (Type)((Dictionary<string, object>)targetResource)[TYPE_FIELD];
//                var memberInfo = type.GetMember(propertyName)[0];
//                var metaTable = dataContext.Mapping.GetTable(type);
//                var dataMember = metaTable.RowType.GetDataMember(memberInfo);

//                var association = dataMember.Association;
//                Debug.Assert(association != null);
//                Debug.Assert(association.ThisKey.Count == association.OtherKey.Count);
//                var count = association.OtherKey.Count;
//                for (var i = 0; i < count; i++)
//                {
//                    Debug.Assert(((Dictionary<string, object>)propertyValue)[TYPE_FIELD] ==
//                                 association.OtherKey[i].Member.DeclaringType);
//                    var n = association.OtherKey[i].Member.Name;
//                    var v = ((Dictionary<string, object>)propertyValue)[n];

//                    n = association.ThisKey[i].Member.Name;
//                    ((Dictionary<string, object>)targetResource)[n] = v;
//                }

//            }

//            /// <summary>
//            /// Adds the given value to the collection
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="resourceToBeAdded">value of the property which needs to be added</param>
//            void IUpdatable.AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
//            {
//                PropertyInfo pi = targetResource.GetType().GetProperty(propertyName);
//                if (pi == null)
//                    throw new Exception("Can't find property");
//                IList collection = (IList)pi.GetValue(targetResource, null);
//                collection.Add(resourceToBeAdded);
//            }

//            /// <summary>
//            /// Removes the given value from the collection
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="resourceToBeRemoved">value of the property which needs to be removed</param>
//            void IUpdatable.RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
//            {
//                PropertyInfo pi = targetResource.GetType().GetProperty(propertyName);
//                if (pi == null)
//                    throw new Exception("Can't find property");
//                IList collection = (IList)pi.GetValue(targetResource, null);
//                collection.Remove(resourceToBeRemoved);
//            }

//            /// <summary>
//            /// Delete the given resource
//            /// </summary>
//            /// <param name="targetResource">resource that needs to be deleted</param>
//            void IUpdatable.DeleteResource(object targetResource)
//            {
//                var item = (Dictionary<string, object>)targetResource;
//                item[METHOD_FIELD] = Methods.Delete;
//            }

//            /// <summary>
//            /// Saves all the pending changes made till now
//            /// </summary>
//            void IUpdatable.SaveChanges()
//            {
//                //顺序 插入、删除、更新
//                dataContext.Connection.Open();
//                var tran = dataContext.Connection.BeginTransaction();
//                dataContext.Transaction = tran;

//                try
//                {
//                    foreach (var item in items.Where(o => (Methods)o[METHOD_FIELD] == Methods.Insert))
//                    {
//                        var type = (Type)item[TYPE_FIELD];
//                        var metaType = dataContext.Mapping.GetMetaType(type);
//                        var insertCommand = CreateInsertCommand(item);
//                        ((SqlProvider)dataContext.Provider).LogCommand(dataContext.Log, insertCommand);
//                        insertCommand.ExecuteNonQuery();

//                        var autoMember = metaType.DBGeneratedIdentityMember;
//                        if (autoMember != null)
//                        {
//                            var identityCommand = CreateIdentityCommand(item);
//                            var result = identityCommand.ExecuteScalar();
//                            item[autoMember.Member.Name] = result;
//                        }
//                    }

//                    foreach (var item in items.Where(o => (Methods)o[METHOD_FIELD] == Methods.Delete))
//                    {
//                        var deleteCommand = CreateDeleteCommand(item);
//                        ((SqlProvider)dataContext.Provider).LogCommand(dataContext.Log, deleteCommand);
//                        deleteCommand.ExecuteNonQuery();
//                    }

//                    foreach (var item in items.Where(o => (Methods)o[METHOD_FIELD] == Methods.Update))
//                    {
//                        var updateCommand = CreateUpdateCommand(item);
//                        ((SqlProvider)dataContext.Provider).LogCommand(dataContext.Log, updateCommand);
//                        updateCommand.ExecuteNonQuery();
//                    }

//                    tran.Commit();
//                }
//                catch (Exception exc)
//                {
//                    tran.Rollback();
//                    throw exc;
//                }
//                finally
//                {
//                    dataContext.Connection.Close();
//                }
//            }

//            private IDbCommand CreateIdentityCommand(Dictionary<string, object> item)
//            {
//                var metaTable = dataContext.Mapping.GetTable((Type)item[TYPE_FIELD]);
//                var tableName = metaTable.TableName;
//                var command = CreateCommand();
//                switch (dataContext.Mapping.ProviderType.Name)
//                {
//                    case "AccessDbProvider":
//                    case "Sql2000Provider":
//                    case "Sql2005Provider":
//                    case "SQLiteProvider":
//                    case "PgsqlProvider":
//                        command.CommandText = "SELECT @@IDENTITY";
//                        break;
//                    case "DB2Provider":
//                        command.CommandText = "PREVVAL FOR SEQ_" + tableName + " FROM " + metaTable.TableName;
//                        break;
//                    case "OracleProvider":
//                        command.CommandText = "SELECT SEQ_" + tableName + ".CURRVAL";
//                        break;
//                    case "FirebirdProvider":
//                        command.CommandText = string.Format("SELECT GEN_ID({0},0)", "SEQ_" + tableName);
//                        break;
//                    case "MySqlProvider":
//                        command.CommandText = "SELECT LAST_INSERT_ID()";
//                        break;
//                }
//                return command;
//            }

//            /// <summary>
//            /// Returns the actual instance of the resource represented by the given resource object
//            /// </summary>
//            /// <param name="resource">object representing the resource whose instance needs to be fetched</param>
//            /// <returns>The actual instance of the resource represented by the given resource object</returns>
//            object IUpdatable.ResolveResource(object resource)
//            {
//                var item = (Dictionary<string, object>)resource;
//                var type = (Type)item[TYPE_FIELD];
//                var obj = Activator.CreateInstance(type);

//                var metaTable = dataContext.Mapping.GetTable(type);
//                var metaMembers = metaTable.RowType.DataMembers.Where(o => item.Keys.Contains(o.Member.Name));
//                foreach (var metaMember in metaMembers)
//                {
//                    metaMember.MemberAccessor.SetBoxedValue(ref obj, item[metaMember.Member.Name]);
//                }
//                return obj;
//            }

//            /// <summary>
//            /// Revert all the pending changes.
//            /// </summary>
//            void IUpdatable.ClearChanges()
//            {
//                // see issue #2 in Code Gallery
//                // No clear way how to do this with LtoS?

//                // Comment out the following line if you'd prefer a silent failure
//                items.Clear();
//            }

//            IDbCommand CreateInsertCommand(Dictionary<string, object> item)
//            {
//                var sb = new StringBuilder();
//                sb.Append("INSERT INTO ");

//                var metaTable = dataContext.Mapping.GetTable((Type)item[TYPE_FIELD]);
//                var obj = Activator.CreateInstance((Type)item[TYPE_FIELD]);
//                sb.AppendLine(metaTable.TableName);

//                var metaMembers = metaTable.RowType.DataMembers.ToList();//.Where(o => item.Keys.Contains(o.Member.Name))
//#if NET35
//                sb.AppendLine("      (" + string.Join(",", metaMembers.Select(o => o.Name).ToArray()) + ")");
//#else
//                sb.AppendLine("      (" + string.Join(",", metaMembers.Select(o => o.Name)) + ")");
//#endif

//                if (metaTable.RowType.DBGeneratedIdentityMember != null)
//                {
//                    switch (dataContext.Mapping.ProviderType.Name)
//                    {
//                        case "DB2Provider":
//                        case "OracleProvider":
//                        case "FirebirdProvider":
//                            metaMembers.Insert(0, metaTable.RowType.DBGeneratedIdentityMember);
//                            break;
//                    }
//                }


//                sb.Append("Values(");
//                var command = CreateCommand();
//                for (var i = 0; i < metaMembers.Count; i++)
//                {
//                    if (metaMembers[i] == metaTable.RowType.DBGeneratedIdentityMember)
//                    {
//                        switch (dataContext.Mapping.ProviderType.Name)
//                        {
//                            case "DB2Provider":
//                                sb.Append(string.Format("NEXTVAL FOR SEQ_{0},", metaTable.TableName));
//                                break;
//                            case "OracleProvider":
//                                sb.Append(string.Format("SEQ_{0}.CURRVAL,", metaTable.TableName));
//                                break;
//                            case "FirebirdProvider":
//                                sb.Append(string.Format("NEXT VALUE FOR SEQ_{0},", metaTable.TableName));
//                                break;

//                        }
//                    }



//                    var parameterName = ParameterPrefix + "p" + i;
//                    object parameterValue;
//                    if (item.Keys.Contains(metaMembers[i].Member.Name) == false)
//                    {
//                        parameterValue = metaMembers[i].MemberAccessor.GetBoxedValue(obj);
//                    }
//                    else
//                    {
//                        parameterValue = item[metaMembers[i].Member.Name];
//                    }

//                    AddParameter(command, parameterName, parameterValue);

//                    sb.Append(parameterName);
//                    if (i != metaMembers.Count - 1)
//                        sb.Append(",");
//                }

//                sb.Append(")");
//                command.CommandText = sb.ToString();

//                return command;
//            }

//            private IDbCommand CreateUpdateCommand(Dictionary<string, object> item)
//            {
//                var command = CreateCommand();

//                var sb = new StringBuilder();
//                sb.Append("UPDATE ");

//                var metaTable = dataContext.Mapping.GetTable((Type)item[TYPE_FIELD]);
//                sb.AppendLine(metaTable.TableName);
//                var metaMembers = metaTable.RowType.DataMembers
//                                           .Where(o => item.Keys.Contains(o.Member.Name) && !o.IsDiscriminator);

//                var updateMembers = metaMembers.Where(o => !o.IsPrimaryKey).ToArray();
//                sb.Append("SET ");
//                for (var i = 0; i < updateMembers.Length; i++)
//                {
//                    var parameterName = ParameterPrefix + "p" + i;
//                    sb.Append(string.Format("{0} = {1}", updateMembers[i].Name, parameterName));
//                    if (i != updateMembers.Length - 1)
//                        sb.Append(",");
//                    else
//                        sb.AppendLine();

//                    AddParameter(command, parameterName, item[updateMembers[i].Member.Name]);
//                }

//                var primaryKeyMembers = metaMembers.Where(o => o.IsPrimaryKey).ToArray();
//                sb.Append("WHERE ");
//                for (var i = 0; i < primaryKeyMembers.Length; i++)
//                {
//                    var parameterName = ParameterPrefix + "p" + (updateMembers.Length + i);
//                    sb.Append(string.Format("{0} = {1}", primaryKeyMembers[i].Name, parameterName));
//                    if (i != primaryKeyMembers.Length - 1)
//                        sb.Append(",");

//                    AddParameter(command, parameterName, item[primaryKeyMembers[i].Member.Name]);
//                }

//                command.CommandText = sb.ToString();
//                return command;
//            }

//            private IDbCommand CreateDeleteCommand(Dictionary<string, object> item)
//            {
//                var command = CreateCommand();

//                var sb = new StringBuilder();
//                sb.Append("DELETE FROM ");

//                var metaTable = dataContext.Mapping.GetTable((Type)item[TYPE_FIELD]);
//                sb.AppendLine(metaTable.TableName);

//                sb.Append("WHERE ");
//                var primaryKeyMembers = metaTable.RowType.DataMembers.Where(o => o.IsPrimaryKey).ToArray();
//                for (var i = 0; i < primaryKeyMembers.Length; i++)
//                {
//                    var parameterName = ParameterPrefix + "p" + i;
//                    sb.Append(string.Format("{0} = {1}", primaryKeyMembers[i].Name, parameterName));
//                    if (i != primaryKeyMembers.Length - 1)
//                        sb.Append(",");

//                    AddParameter(command, parameterName, item[primaryKeyMembers[i].Member.Name]);
//                }
//                command.CommandText = sb.ToString();
//                return command;
//            }

//            private IDbCommand CreateCommand()
//            {
//                var command = dataContext.Connection.CreateCommand();
//                command.Transaction = dataContext.Transaction;
//                return command;
//            }

//            private void AddParameter(IDbCommand command, string parameterName, object parameterValue)
//            {
//                var p = command.CreateParameter();
//                p.ParameterName = parameterName;
//                p.Value = parameterValue;
//                command.Parameters.Add(p);
//            }


//            string ParameterPrefix
//            {
//                get
//                {
//                    if (dataContext.Mapping.ProviderType.Name == "OracleProvider")
//                    {
//                        return ":";
//                    }
//                    return "@";
//                }
//            }


//        }
//        #endregion

//        #region Subclass DataContextUpdater
//        class DataContextUpdater : IUpdatable
//        {
//            private readonly DataContext dataContext;

//            public DataContextUpdater(DataContext dataContext)
//            {
//                this.dataContext = dataContext;
//            }
//            /// <summary>
//            /// Creates the resource of the given type and belonging to the given container
//            /// </summary>
//            /// <param name="containerName">container name to which the resource needs to be added</param>
//            /// <param name="fullTypeName">full type name i.e. Namespace qualified type name of the resource</param>
//            /// <returns>object representing a resource of given type and belonging to the given container</returns>
//            object IUpdatable.CreateResource(string containerName, string fullTypeName)
//            {
//                Type t = Type.GetType(fullTypeName, true);
//                ITable table = dataContext.GetTable(t);
//                object resource = Activator.CreateInstance(t);
//                table.InsertOnSubmit(resource);
//                return resource;
//            }

//            /// <summary>
//            /// Gets the resource of the given type that the query points to
//            /// </summary>
//            /// <param name="query">query pointing to a particular resource</param>
//            /// <param name="fullTypeName">full type name i.e. Namespace qualified type name of the resource</param>
//            /// <returns>object representing a resource of given type and as referenced by the query</returns>
//            object IUpdatable.GetResource(IQueryable query, string fullTypeName)
//            {
//                object resource = query.Cast<object>().SingleOrDefault();

//                // fullTypeName can be null for deletes
//                if (fullTypeName != null && resource.GetType().FullName != fullTypeName)
//                    throw new Exception("Unexpected type for resource");
//                return resource;
//            }


//            /// <summary>
//            /// Resets the value of the given resource to its default value
//            /// </summary>
//            /// <param name="resource">resource whose value needs to be reset</param>
//            /// <returns>same resource with its value reset</returns>
//            object IUpdatable.ResetResource(object resource)
//            {
//                Type t = resource.GetType();
//                MetaTable table = dataContext.Mapping.GetTable(t);
//                object dummyResource = Activator.CreateInstance(t);
//                foreach (var member in table.RowType.DataMembers)
//                {
//                    if (!member.IsPrimaryKey && !member.IsDeferred &&
//                        !member.IsAssociation && !member.IsDbGenerated)
//                    {
//                        object defaultValue = member.MemberAccessor.GetBoxedValue(dummyResource);
//                        member.MemberAccessor.SetBoxedValue(ref resource, defaultValue);
//                    }
//                }
//                return resource;
//            }

//            /// <summary>
//            /// Sets the value of the given property on the target object
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="propertyValue">value of the property</param>
//            void IUpdatable.SetValue(object targetResource, string propertyName, object propertyValue)
//            {
//                MetaTable table = dataContext.Mapping.GetTable(targetResource.GetType());
//                MetaDataMember member = table.RowType.DataMembers.Single(x => x.Name == propertyName);
//                member.MemberAccessor.SetBoxedValue(ref targetResource, propertyValue);

//            }

//            /// <summary>
//            /// Gets the value of the given property on the target object
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <returns>the value of the property for the given target resource</returns>
//            object IUpdatable.GetValue(object targetResource, string propertyName)
//            {
//                MetaTable table = dataContext.Mapping.GetTable(targetResource.GetType());
//                MetaDataMember member = table.RowType.DataMembers.Single(x => x.Name == propertyName);
//                return member.MemberAccessor.GetBoxedValue(targetResource);
//            }

//            /// <summary>
//            /// Sets the value of the given reference property on the target object
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="propertyValue">value of the property</param>
//            void IUpdatable.SetReference(object targetResource, string propertyName, object propertyValue)
//            {
//                ((IUpdatable)this).SetValue(targetResource, propertyName, propertyValue);
//            }

//            /// <summary>
//            /// Adds the given value to the collection
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="resourceToBeAdded">value of the property which needs to be added</param>
//            void IUpdatable.AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
//            {
//                PropertyInfo pi = targetResource.GetType().GetProperty(propertyName);
//                if (pi == null)
//                    throw new Exception("Can't find property");
//                IList collection = (IList)pi.GetValue(targetResource, null);
//                collection.Add(resourceToBeAdded);
//            }

//            /// <summary>
//            /// Removes the given value from the collection
//            /// </summary>
//            /// <param name="targetResource">target object which defines the property</param>
//            /// <param name="propertyName">name of the property whose value needs to be updated</param>
//            /// <param name="resourceToBeRemoved">value of the property which needs to be removed</param>
//            void IUpdatable.RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
//            {
//                PropertyInfo pi = targetResource.GetType().GetProperty(propertyName);
//                if (pi == null)
//                    throw new Exception("Can't find property");
//                IList collection = (IList)pi.GetValue(targetResource, null);
//                collection.Remove(resourceToBeRemoved);
//            }

//            /// <summary>
//            /// Delete the given resource
//            /// </summary>
//            /// <param name="targetResource">resource that needs to be deleted</param>
//            void IUpdatable.DeleteResource(object targetResource)
//            {
//                ITable table = dataContext.GetTable(targetResource.GetType());
//                table.DeleteOnSubmit(targetResource);
//            }

//            /// <summary>
//            /// Saves all the pending changes made till now
//            /// </summary>
//            void IUpdatable.SaveChanges()
//            {
//                dataContext.SubmitChanges();
//            }

//            /// <summary>
//            /// Returns the actual instance of the resource represented by the given resource object
//            /// </summary>
//            /// <param name="resource">object representing the resource whose instance needs to be fetched</param>
//            /// <returns>The actual instance of the resource represented by the given resource object</returns>
//            object IUpdatable.ResolveResource(object resource)
//            {
//                return resource;
//            }

//            /// <summary>
//            /// Revert all the pending changes.
//            /// </summary>
//            void IUpdatable.ClearChanges()
//            {
//                // see issue #2 in Code Gallery
//                // No clear way how to do this with LtoS?

//                // Comment out the following line if you'd prefer a silent failure
//                throw new NotSupportedException();

//            }
//        }
//        #endregion

//        private IUpdatable updater;

//        public IUpdatable Updater
//        {
//            get
//            {
//                if (updater == null)
//                    updater = new SqlUpdater(this);

//                return updater;
//            }
//        }


//        object IUpdatable.CreateResource(string containerName, string fullTypeName)
//        {
//            return Updater.CreateResource(containerName, fullTypeName);
//        }

//        object IUpdatable.GetResource(IQueryable query, string fullTypeName)
//        {
//            return Updater.GetResource(query, fullTypeName);
//        }

//        object IUpdatable.ResetResource(object resource)
//        {
//            return Updater.ResetResource(resource);
//        }

//        void IUpdatable.SetValue(object targetResource, string propertyName, object propertyValue)
//        {
//            Updater.SetValue(targetResource, propertyName, propertyValue);
//        }

//        object IUpdatable.GetValue(object targetResource, string propertyName)
//        {
//            return Updater.GetValue(targetResource, propertyName);
//        }

//        void IUpdatable.SetReference(object targetResource, string propertyName, object propertyValue)
//        {
//            Updater.SetReference(targetResource, propertyName, propertyValue);
//        }

//        void IUpdatable.AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
//        {
//            Updater.AddReferenceToCollection(targetResource, propertyName, resourceToBeAdded);
//        }

//        void IUpdatable.RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
//        {
//            Updater.RemoveReferenceFromCollection(targetResource, propertyName, resourceToBeRemoved);
//        }

//        void IUpdatable.DeleteResource(object targetResource)
//        {
//            Updater.DeleteResource(targetResource);
//        }

//        void IUpdatable.SaveChanges()
//        {
//            Updater.SaveChanges();
//        }

//        object IUpdatable.ResolveResource(object resource)
//        {
//            return Updater.ResolveResource(resource);
//        }

//        void IUpdatable.ClearChanges()
//        {
//            Updater.ClearChanges();

//        }


//    }
//} 
#endregion
