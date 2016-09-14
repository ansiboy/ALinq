using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ALinq.SqlClient;

namespace ALinq.Mapping
{

    internal class DynamicModel : MetaModel
    {
        #region SubClass MetaDataMemberBuilder
        //private class MetaDataMemberBuilder : ALinq.SqlClient.ExpressionVisitor
        //{
        //    private DynamicModel model;
        //    private Dictionary<Expression, Type> targetTypes;

        //    public MetaDataMemberBuilder(DynamicModel model)
        //    {
        //        //this.sql = sql;
        //        this.model = model;
        //        targetTypes = new Dictionary<Expression, Type>();
        //    }

        //    public Expression Build(Expression node)
        //    {
        //        return Visit(node);
        //    }

        //    public override Expression VisitUnary(UnaryExpression u)
        //    {
        //        targetTypes[u.Operand] = u.Type;

        //        var node = base.VisitUnary(u);
        //        return node;
        //    }

        //    public override Expression VisitMethodCall(MethodCallExpression mc)
        //    {
        //        if (mc.Method.Name == "get_Item" && mc.Arguments.Count == 1 && mc.Object != null)
        //        {
        //            var arg = mc.Arguments[0] as ConstantExpression;
        //            if (arg != null && arg.Value is string)
        //            {
        //                var metaType = model.GetMetaType(mc.Object.Type).InheritanceRoot as DynamicRootType;
        //                if (metaType != null)// && metaType.IndexerDataMember.IsPersistent)
        //                {
        //                    Type type;
        //                    if (!targetTypes.TryGetValue(mc, out type))
        //                        type = mc.Method.ReturnType;

        //                    if (type.IsValueType && !TypeSystem.IsNullableType(type))
        //                        type = typeof(Nullable<>).MakeGenericType(type);


        //                    var mf = new IndexerMemberInfo(mc.Method, (string)arg.Value, type);
        //                    metaType.AddOrUpdateDataMembers(metaType, mf);
        //                }
        //            }

        //        }

        //        return base.VisitMethodCall(mc);
        //    }




        //}
        #endregion

        private readonly MetaModel source;
        private readonly Dictionary<MetaTable, DynamicMetaTable> metaTables;
        private readonly Dictionary<MetaType, DynamicMetaType> metaTypes;

        private readonly DynamicMappingSource mappingSource;
        //private readonly MetaDataMemberBuilder modelUpdater;
        //private readonly Dictionary<string, DynamicMetaTable> tablesByName;
        //private ModuleBuilder moduleBuilder;
        //private AttributedMetaModel extendModel;

        internal DynamicModel(MetaModel source, DynamicMappingSource mappingSource)
        {
            Debug.Assert(source is DynamicModel == false);
            this.source = source;
            //this.dataMembers = new Dictionary<Type, List<IndexPropertyDataMember>>();
            metaTables = new Dictionary<MetaTable, DynamicMetaTable>();
            metaTypes = new Dictionary<MetaType, DynamicMetaType>();
            this.mappingSource = mappingSource;
            //this.modelUpdater = new MetaDataMemberBuilder(this);
        }

        public override MetaFunction GetFunction(MethodInfo method)
        {
            return source.GetFunction(method);
        }

        public override IEnumerable<MetaFunction> GetFunctions()
        {
            return source.GetFunctions();
        }


        //private static Type GetMappingType(Type type)
        //{
        //    //判断如果类为系统自动生成的，则采用接口的映射。
        //    if( type.Name.StartsWith("<>"))
        //    {
        //        Debug.Assert(type.IsClass);
        //        Debug.Assert(type.GetInterfaces().Length == 1);
        //        return type.GetInterfaces()[0];
        //    }
        //    return type;
        //}

        public override MetaType GetMetaType(Type type)
        {
            //type = GetMappingType(type);

            MetaType metaType = source.GetMetaType(type);

            //if (metaType == null)
            //    metaType = ExtendModel.GetMetaType(type);

            metaType = GetMetaTypeBySource(metaType);
            Debug.Assert(metaType != null);
            return metaType;
        }

        internal DynamicMetaType GetMetaTypeBySource(MetaType metaType)
        {
            if (metaType == null)
                return null;

            //TODO:加锁
            Debug.Assert(metaType is DynamicMetaType == false);
            DynamicMetaType myMetaType;
            if (!metaTypes.TryGetValue(metaType, out myMetaType))
            {
                if (metaType == metaType.InheritanceRoot)
                    myMetaType = new DynamicRootType(this, metaType);
                else
                    myMetaType = new DynamicMetaType(this, metaType);

                metaTypes[metaType] = myMetaType;
            }
            return myMetaType;
        }

        internal DynamicMetaTable GetMetaTableBySource(MetaTable metaTable)
        {
            if (metaTable == null)
                return null;

            //TODO:加锁
            Debug.Assert(metaTable is DynamicMetaTable == false);
            DynamicMetaTable myMetaTable;
            if (!metaTables.TryGetValue(metaTable, out myMetaTable))
            {
                myMetaTable = new DynamicMetaTable(this, metaTable);
                metaTables[metaTable] = myMetaTable;
            }
            return myMetaTable;
        }

        public override MetaTable GetTable(Type rowType)
        {
            //rowType = GetMappingType(rowType);
            MetaTable metaTable = source.GetTable(rowType);
            //if (metaTable == null)
            //    metaTable = ExtendModel.GetTable(rowType);

            metaTable = GetMetaTableBySource(metaTable);
            if (metaTable == null)
            {
                throw ALinq.Error.TypeIsNotMarkedAsTable(rowType);
            }

            return metaTable;
        }

        public override IEnumerable<MetaTable> GetTables()
        {
            var tables = source.GetTables();//.Union(ExtendModel.GetTables());
            return tables.Select(o => GetMetaTableBySource(o)).Cast<MetaTable>();
        }

        public override Type ContextType
        {
            get { return source.ContextType; }
        }

        public override string DatabaseName
        {
            get { return source.DatabaseName; }
        }

        public override MappingSource MappingSource
        {
            get { return this.mappingSource; }
        }

        public override Type ProviderType
        {
            get { return source.ProviderType; }
        }

        public MetaModel Source
        {
            get { return this.source; }
        }

        #region MyRegion
        //internal void UpdateBy(Expression query)
        //{
        //    modelUpdater.Build(query);
        //} 
        #endregion

        #region MyRegion
        //internal MetaTable GetTable(string tableName, Type type)
        //{
        //    if (!type.IsInterface)
        //        throw ALinq.Mapping.Error.MustBeInterface(type);

        //    if (type.IsNotPublic)
        //        throw ALinq.Mapping.Error.CannotAccessInterface(type);


        //    //var metaTable = ExtendModel.GetTables().Where(o => o.TableName == tableName)
        //    //                           .FirstOrDefault();
        //    var metaTable = source.GetTables().FirstOrDefault(o => o.TableName == tableName);
        //    if (metaTable == null)
        //    {

        //        var mb = GetAssemblyBuilder();

        //        //1、Create 类以及 ClassAttribute
        //        var className = "<>TempClass";//TODO:命名
        //        var tableAttribute = new TableAttribute { Name = tableName };

        //        var ctorInfo = typeof(TableAttribute).GetConstructor(new Type[] { });
        //        var p1 = typeof(TableAttribute).GetProperty("Name");
        //        var cb = new CustomAttributeBuilder(ctorInfo, new object[] { }, new[] { p1 }, new object[] { tableName });

        //        var tb = mb.DefineType(className, TypeAttributes.Class | TypeAttributes.Public, null, null);//new[] { type }
        //        tb.SetCustomAttribute(cb);

        //        var fields = type.GetFields();
        //        foreach (var field in fields)
        //        {
        //            tb.DefineField(field.Name, field.FieldType, FieldAttributes.Public);
        //        }

        //        var properties = type.GetProperties();
        //        foreach (var property in properties)
        //        {
        //            var f = tb.DefineField("<>" + property.Name, property.PropertyType, FieldAttributes.Private);

        //            var ma = MethodAttributes.Public | MethodAttributes.Virtual;
        //            ctorInfo = typeof(ColumnAttribute).GetConstructor(new Type[] { });
        //            cb = new CustomAttributeBuilder(ctorInfo, new object[] { });
        //            var pb = tb.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
        //            pb.SetCustomAttribute(cb);

        //            var g = tb.DefineMethod("get_" + property.Name, ma, pb.PropertyType, null);
        //            var gen = g.GetILGenerator();
        //            gen.Emit(OpCodes.Ldarg_0);
        //            gen.Emit(OpCodes.Ldfld, f);
        //            gen.Emit(OpCodes.Ret);

        //            var s = tb.DefineMethod("set_" + property.Name, ma, pb.PropertyType, null);
        //            gen = s.GetILGenerator();
        //            gen.Emit(OpCodes.Ldarg_0);
        //            gen.Emit(OpCodes.Ldarg_1);
        //            gen.Emit(OpCodes.Stfld, f);
        //            gen.Emit(OpCodes.Ret);


        //            pb.SetGetMethod(g);
        //            pb.SetSetMethod(s);
        //        }

        //        //tb.SetCustomAttribute()
        //        //2、创建属性以及 ColumnAttribute

        //        var rowType = tb.CreateType();
        //        //rowType.GetCustomAttributes(true);

        //        metaTable = source.GetTable(rowType); //new UnmappedTable(this, rowType, tableName);
        //        metaTable = GetMetaTableBySource(metaTable);
        //        return metaTable;

        //    }
        //    return metaTable;
        //}

        //ModuleBuilder GetAssemblyBuilder()
        //{
        //    if (moduleBuilder == null)
        //    {
        //        AppDomain myCurrentDomain = AppDomain.CurrentDomain;
        //        AssemblyName myAssemblyName = new AssemblyName();
        //        myAssemblyName.Name = "TempAssembly";//TODO:命名。
        //        var ab = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.Run);

        //        var modelName = "TempModule";//TODO:命名
        //        moduleBuilder = ab.DefineDynamicModule(modelName);
        //    }
        //    return moduleBuilder;
        //}

        //TypeBuilder CreateTypeBuilder(ModuleBuilder mb)
        //{
        //    var className = "TempClass";//TODO:命名
        //    return mb.DefineType(className);
        //} 
        #endregion



        #region MyRegion
        //internal void UpdateBy(object item)
        //{
        //    if (item == null)
        //        throw new ArgumentNullException("item");

        //    var type = item.GetType();
        //    var rowType = GetMetaType(type) as DynamicRootType;
        //    if (rowType != null && rowType.IndexerDataMember != null)
        //    {
        //        var sm = rowType.IndexerDataMember.StorageMember as FieldInfo;
        //        var t = typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object));
        //        if (sm != null)
        //        {
        //            var values = sm.GetValue(item) as IDictionary<string, object>;
        //            if (values != null)
        //            {

        //                foreach (var keyValue in values)
        //                {
        //                    var key = keyValue.Key;
        //                    var value = keyValue.Value;


        //                    DynamicMetaDataMember dataMember;
        //                    if (!rowType.ExtendDataMembers.TryGetValue(key, out dataMember))
        //                    {
        //                        var t1 = typeof(object);
        //                        if (value != null)
        //                            t1 = value.GetType();

        //                        var memberInfo = new IndexerMemberInfo(type, key, t1);
        //                        dataMember = new DynamicMetaDataMember(rowType, memberInfo, rowType.DataMembers.Count);
        //                        rowType.ExtendDataMembers.Add(key, dataMember);
        //                    }

        //                }
        //            }
        //        }
        //    }
        //} 
        #endregion
    }

}