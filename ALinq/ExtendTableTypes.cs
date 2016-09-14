using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ALinq.Mapping;

namespace ALinq
{
    class ExtendTableTypes
    {
        public class ItemKey
        {
            public string TableName;
            public Type InterfaceType;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(ItemKey)) return false;
                return Equals((ItemKey)obj);
            }

            public bool Equals(ItemKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.TableName, TableName) && Equals(other.InterfaceType, InterfaceType);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((TableName != null ? TableName.GetHashCode() : 0) * 397) ^ (InterfaceType != null ? InterfaceType.GetHashCode() : 0);
                }
            }
        }

        private Dictionary<ItemKey, Type> types;
        private ModuleBuilder moduleBuilder;

        internal ExtendTableTypes()
        {
            types = new Dictionary<ItemKey, Type>();
        }


        internal Type GetType(string tableName, Type interfaceType)
        {
            Type type;
            var key = new ItemKey { TableName = tableName, InterfaceType = interfaceType };
            if (types.TryGetValue(key, out type))
                return type;

            //TODO:加锁
            type = CreateEntityType(tableName, interfaceType);
            types[key] = type;
            return type;
        }

        private Type CreateEntityType(string tableName, Type baseInterface)
        {
            var type = baseInterface;
            var mb = GetAssemblyBuilder();

            //1、Create 类以及 ClassAttribute
            var className = "<>" + baseInterface.Name + types.Count;
            //var tableAttribute = new TableAttribute { Name = tableName };

            var attrTable = type.GetCustomAttributes(false).OfType<TableAttribute>().FirstOrDefault();
            if (attrTable == null)
            {
                attrTable = new TableAttribute { Name = tableName };
            }

            var tabps = typeof(TableAttribute).GetProperties().Where(o => o.CanWrite).ToArray();
            var tabvs = tabps.Select(o => o.GetValue(attrTable, null)).ToArray();

            var ctorInfo = typeof(TableAttribute).GetConstructor(Type.EmptyTypes);
            var cb = new CustomAttributeBuilder(ctorInfo, Type.EmptyTypes, tabps, tabvs);

            var tb = mb.DefineType(className, TypeAttributes.Class | TypeAttributes.Public, typeof(object), new[] { type });

            tb.SetCustomAttribute(cb);

            FieldBuilder itemsField = null;
            var properties = type.GetProperties();
            var fields = new List<FieldBuilder>(); //new Dictionary<string, FieldBuilder>();

            foreach (var property in properties)
            {
                var pb = tb.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, Type.EmptyTypes);
                pb.SetCustomAttribute(cb);
                var ma = MethodAttributes.Public | MethodAttributes.Virtual;

                var indexParameters = property.GetIndexParameters();
                if (indexParameters.Length == 0)
                {
                    var f = tb.DefineField("<>" + property.Name, property.PropertyType, FieldAttributes.Private);
                    fields.Add(f);
                    var attrColumn = property.GetCustomAttributes(false).OfType<ColumnAttribute>().FirstOrDefault();
                    if (attrColumn == null)
                    {
                        attrColumn = new ColumnAttribute();
                        attrColumn.UpdateCheck = UpdateCheck.Never;
                        attrColumn.Storage = f.Name;
                    }

                    var ps = typeof(ColumnAttribute).GetProperties().Where(o => o.CanWrite).ToArray();
                    var vs = ps.Select(o => o.GetValue(attrColumn, null)).ToArray();

                    ctorInfo = typeof(ColumnAttribute).GetConstructor(new Type[] { });
                    cb = new CustomAttributeBuilder(ctorInfo, new object[] { }, ps, vs);

                    var g = tb.DefineMethod("get_" + property.Name, ma, pb.PropertyType, Type.EmptyTypes);
                    var gen = g.GetILGenerator();
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, f);
                    gen.Emit(OpCodes.Ret);
                    pb.SetGetMethod(g);

                    var s = tb.DefineMethod("set_" + property.Name, ma, null, new[] { pb.PropertyType });
                    gen = s.GetILGenerator();
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Stfld, f);
                    gen.Emit(OpCodes.Ret);
                    pb.SetSetMethod(s);
                    //}
                }
                else if (indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(string) &&
                         property.PropertyType == typeof(object))
                {
                    itemsField = tb.DefineField("<>" + property.Name, typeof(Dictionary<string, object>), FieldAttributes.Private);

                    var g = tb.DefineMethod("get_" + property.Name, ma, pb.PropertyType, new[] { typeof(string) });
                    var gen = g.GetILGenerator();


                    var labGetItem = gen.DefineLabel();
                    var fieldLables = fields.Select(o => gen.DefineLabel()).ToArray();

                    for (var i = 0; i < fields.Count;i++ )
                    {
                        var field = fields[i];
                        var propertyName = field.Name.Substring(2);

                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Ldstr, propertyName);
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Brtrue, fieldLables[i]);
                    }

                    gen.MarkLabel(labGetItem);
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, itemsField);
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Callvirt, itemsField.FieldType.GetMethod("get_Item"));
                    gen.Emit(OpCodes.Ret);

                    for (var i = 0; i < fieldLables.Length; i++)
                    {
                        gen.MarkLabel(fieldLables[i]);
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldfld, fields[i]);
                        gen.Emit(OpCodes.Ret);
                    }


                    pb.SetGetMethod(g);

                    var s = tb.DefineMethod("set_" + property.Name, ma, null, new[] { typeof(string), pb.PropertyType });
                    gen = s.GetILGenerator();
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, itemsField);
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Ldarg_2);
                    gen.Emit(OpCodes.Callvirt, itemsField.FieldType.GetMethod("set_Item"));
                    //gen.Emit(OpCodes.Stfld, f);
                    gen.Emit(OpCodes.Ret);
                    pb.SetSetMethod(s);
                }
                else
                {
                    throw Error.CannotContainsProperty(baseInterface, property);
                }

            }

            //创建方法
            var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { });
            var genIL = ctorBuilder.GetILGenerator();
            if (itemsField != null)
            {
                //var b
                //itemsField.FieldType.GetConstructor()
                genIL.Emit(OpCodes.Ldarg_0);
                genIL.Emit(OpCodes.Newobj, itemsField.FieldType.GetConstructor(Type.EmptyTypes));
                genIL.Emit(OpCodes.Stfld, itemsField);
            }

            genIL.Emit(OpCodes.Ret);

            var methods = baseInterface.GetMethods();
            //foreach (var method in methods)
            {
                //var parameters = method.GetParameters();
                //if (method.ReturnType != baseInterface && parameters.Length != 1 && method.IsGenericMethod == false)
                //throw Error.CannotContainsMethod(baseInterface, method);

                //var attr = MethodAttributes.Public | MethodAttributes.Virtual;
                //var methodBuilder = tb.DefineMethod(method.Name, attr, baseInterface, Type.EmptyTypes);
                //var gen = methodBuilder.GetILGenerator();
                //gen.Emit(OpCodes.Ldarg_0);
                //gen.Emit(OpCodes.Newobj, ctorBuilder);
                //gen.Emit(OpCodes.Ret);
            }

            var rowType = tb.CreateType();
#if DEBUG
            var a = Activator.CreateInstance(rowType);
#endif
            return rowType;
        }

        ModuleBuilder GetAssemblyBuilder()
        {
            if (moduleBuilder == null)
            {
                AppDomain myCurrentDomain = AppDomain.CurrentDomain;
                AssemblyName myAssemblyName = new AssemblyName();
                var random = new Random();
                var v1 = random.Next(1, 10000);
                var v2 = random.Next(1, 10000);
                myAssemblyName.Name = "<>" + v1 + "_" + v2;
                var ab = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.Run);

                var modelName = "$" + myAssemblyName.Name;
                moduleBuilder = ab.DefineDynamicModule(modelName);
            }
            return moduleBuilder;
        }


    }
}
