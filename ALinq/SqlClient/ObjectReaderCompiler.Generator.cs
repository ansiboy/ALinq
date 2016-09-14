using System;
using System.Collections.Generic;
using System.IO;
using ALinq;
using System.Linq;
using System.Reflection;
using ALinq.Mapping;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using ALinq.SqlClient.Implementation;
using System.Data.Common;
using System.Collections;
using System.Diagnostics;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler
    {
        internal class DefenseCrack
        {
            internal static bool IsDateToValidate(DateTime date)
            {
                double detal;
                var b = IsDateShouldValidate(date, out detal);
                if (detal <= 0)
                    return false;

                var radom = new Random();
                double v = radom.Next(0, 1000);
                //v = v * (new Random().Next(1, 100) / 100d);
                if (v < detal)
                {
                    return true;
                }

                return false;
            }

            internal static bool IsDateShouldValidate(DateTime date, out double detal)
            {
                //24157 = 2013 * 12 + 1，即起始日期 2013年-1月，转换成月份。
                var x1 = 24157;

                var x = date.Year * 12 + date.Month;
                detal = Math.Pow(x - x1, 2) - 24 * (x - x1);

                if (detal > 0)
                    return true;

                return false;
            }
        }
        // Nested Types
        private class Generator
        {
            // Fields
            private Dictionary<MetaAssociation, int> associationSubQueries;
            private readonly ObjectReaderCompiler compiler;
            private readonly Type elementType;
            private ILGenerator gen;
            private List<object> globals;
            private LocalBuilder locDataReader;
            private List<NamedColumn> namedColumns;
            private static readonly Type[] readMethodSignature = new[] { typeof(int) };

            private readonly SideEffectChecker sideEffectChecker =
                new SideEffectChecker();

            // Methods
            internal Generator(ObjectReaderCompiler compiler, Type elementType)
            {
                this.compiler = compiler;
                this.elementType = elementType;
                associationSubQueries = new Dictionary<MetaAssociation, int>();
            }

            private int AddGlobal(Type type, object value)
            {
                int count = globals.Count;
                if (type.IsValueType)
                {
                    globals.Add(Activator.CreateInstance(typeof(StrongBox<>).MakeGenericType(new[] { type }), new[] { value }));
                    return count;
                }
                globals.Add(value);
                return count;
            }

            private int AllocateLocal()
            {
                return Locals++;
            }

            private Type Generate(SqlNode node)
            {
                return Generate(node, null);
            }

            private Type Generate(SqlNode node, LocalBuilder locInstance)
            {
                switch (node.NodeType)
                {
                    case SqlNodeType.ClientArray:
                        return GenerateClientArray((SqlClientArray)node);

                    case SqlNodeType.ClientCase:
                        return GenerateClientCase((SqlClientCase)node, false, locInstance);

                    case SqlNodeType.ClientParameter:
                        return GenerateClientParameter((SqlClientParameter)node);

                    case SqlNodeType.ClientQuery:
                        return GenerateClientQuery((SqlClientQuery)node, locInstance);

                    case SqlNodeType.ColumnRef:
                        return GenerateColumnReference((SqlColumnRef)node);

                    case SqlNodeType.DiscriminatedType:
                        return GenerateDiscriminatedType((SqlDiscriminatedType)node);

                    case SqlNodeType.Lift:
                        return GenerateLift((SqlLift)node);

                    case SqlNodeType.Link:
                        return GenerateLink((SqlLink)node, locInstance);

                    case SqlNodeType.Grouping:
                        return GenerateGrouping((SqlGrouping)node);

                    case SqlNodeType.JoinedCollection:
                        return GenerateJoinedCollection((SqlJoinedCollection)node);

                    case SqlNodeType.MethodCall:
                        return GenerateMethodCall((SqlMethodCall)node);

                    case SqlNodeType.Member:
                        return GenerateMember((SqlMember)node);

                    case SqlNodeType.OptionalValue:
                        return GenerateOptionalValue((SqlOptionalValue)node);

                    case SqlNodeType.OuterJoinedValue:
                        return Generate(((SqlUnary)node).Operand);

                    case SqlNodeType.SearchedCase:
                        return GenerateSearchedCase((SqlSearchedCase)node);

                    case SqlNodeType.New:
                        return GenerateNew((SqlNew)node);

                    case SqlNodeType.Value:
                        return GenerateValue((SqlValue)node);

                    case SqlNodeType.ValueOf:
                        return GenerateValueOf((SqlUnary)node);

                    case SqlNodeType.UserColumn:
                        return GenerateUserColumn((SqlUserColumn)node);

                    case SqlNodeType.TypeCase:
                        return GenerateTypeCase((SqlTypeCase)node);
                }
                throw Error.CouldNotTranslateExpressionForReading(node.SourceExpression);
            }

            private void GenerateAccessArguments()
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, compiler.argsField);
            }

            private void GenerateAccessBufferReader()
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, compiler.bufferReaderField);
            }

            private void GenerateAccessDataReader()
            {
                gen.Emit(OpCodes.Ldloc, locDataReader);
            }

            private void GenerateAccessGlobals()
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, compiler.globalsField);
            }

            private void GenerateAccessOrdinals()
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, compiler.ordinalsField);
            }

            private Type GenerateArrayAccess(Type type, bool address)
            {
                if (!type.IsEnum && (Type.GetTypeCode(type) == TypeCode.Int32))
                {
                    gen.Emit(OpCodes.Ldelem_I4);
                }
                return type;
            }

            private void GenerateArrayAssign(Type type)
            {
                if (!type.IsEnum)
                {
                    TypeCode typeCode = Type.GetTypeCode(type);
                    if (type.IsValueType)
                    {
                        gen.Emit(OpCodes.Stelem, type);
                    }
                    else
                    {
                        gen.Emit(OpCodes.Stelem_Ref);
                    }
                }
            }

            private void GenerateAssignDeferredEntitySet(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr,
                                                         LocalBuilder locStoreInMember)
            {
                MemberInfo mi = mm.StorageMember ?? mm.Member;
                Type memberType = TypeSystem.GetMemberType(mi);
                Label label = gen.DefineLabel();
                Type type2 = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());
                bool flag = HasSideEffect(expr);
                if ((locStoreInMember != null) && !flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                }
                Type localType = GenerateDeferredSource(expr, locInstance);
                LocalBuilder local = gen.DeclareLocal(localType);
                gen.Emit(OpCodes.Stloc, local);
                if ((locStoreInMember != null) && flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                }
                if ((mi is FieldInfo) || ((mi is PropertyInfo) && ((PropertyInfo)mi).CanWrite))
                {
                    Label label2 = gen.DefineLabel();
                    GenerateLoadForMemberAccess(locInstance);
                    GenerateLoadMember(mi);
                    gen.Emit(OpCodes.Ldnull);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, label2);
                    GenerateLoadForMemberAccess(locInstance);
                    ConstructorInfo constructor = memberType.GetConstructor(Type.EmptyTypes);
                    gen.Emit(OpCodes.Newobj, constructor);
                    GenerateStoreMember(mi);
                    gen.MarkLabel(label2);
                }
                GenerateLoadForMemberAccess(locInstance);
                GenerateLoadMember(mi);
                gen.Emit(OpCodes.Ldloc, local);
                MethodInfo meth = memberType.GetMethod("SetSource",
                                                       BindingFlags.NonPublic | BindingFlags.Public |
                                                       BindingFlags.Instance, null, new[] { type2 }, null);
                gen.Emit(GetMethodCallOpCode(meth), meth);
                gen.MarkLabel(label);
            }

            private void GenerateAssignDeferredReference(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr,
                                                         LocalBuilder locStoreInMember)
            {
                MemberInfo mi = mm.StorageMember ?? mm.Member;
                Type memberType = TypeSystem.GetMemberType(mi);
                Label label = gen.DefineLabel();
                Type type2 = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());
                bool flag = HasSideEffect(expr);
                if ((locStoreInMember != null) && !flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                }
                Type localType = GenerateDeferredSource(expr, locInstance);
                LocalBuilder local = gen.DeclareLocal(localType);
                gen.Emit(OpCodes.Stloc, local);
                if ((locStoreInMember != null) && flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                }
                GenerateLoadForMemberAccess(locInstance);
                gen.Emit(OpCodes.Ldloc, local);
                ConstructorInfo con =
                    memberType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                                              new[] { type2 }, null);
                gen.Emit(OpCodes.Newobj, con);
                GenerateStoreMember(mi);
                gen.MarkLabel(label);
            }

            private void GenerateAssignEntitySet(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr,
                                                 LocalBuilder locStoreInMember)
            {
                MemberInfo mi = mm.StorageMember ?? mm.Member;
                Type memberType = TypeSystem.GetMemberType(mi);
                Label label = gen.DefineLabel();
                Type type2 = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());
                bool flag = HasSideEffect(expr);
                if ((locStoreInMember != null) && !flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                }
                Type localType = Generate(expr, mm.DeclaringType.IsEntity ? locInstance : null);
                LocalBuilder local = gen.DeclareLocal(localType);
                gen.Emit(OpCodes.Stloc, local);
                if ((locStoreInMember != null) && flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                }
                if ((mi is FieldInfo) || ((mi is PropertyInfo) && ((PropertyInfo)mi).CanWrite))
                {
                    Label label2 = gen.DefineLabel();
                    GenerateLoadForMemberAccess(locInstance);
                    GenerateLoadMember(mi);
                    gen.Emit(OpCodes.Ldnull);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, label2);
                    GenerateLoadForMemberAccess(locInstance);
                    ConstructorInfo constructor = memberType.GetConstructor(Type.EmptyTypes);
                    gen.Emit(OpCodes.Newobj, constructor);
                    GenerateStoreMember(mi);
                    gen.MarkLabel(label2);
                }
                GenerateLoadForMemberAccess(locInstance);
                GenerateLoadMember(mi);
                gen.Emit(OpCodes.Ldloc, local);
                MethodInfo meth = memberType.GetMethod("Assign",
                                                       BindingFlags.NonPublic | BindingFlags.Public |
                                                       BindingFlags.Instance, null, new[] { type2 }, null);
                gen.Emit(GetMethodCallOpCode(meth), meth);
                gen.MarkLabel(label);
            }



            private void GenerateAssignValue(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr,
                                             LocalBuilder locStoreInMember)
            {
                MemberInfo member = mm.StorageMember ?? mm.Member;
                if (!IsAssignable(member))
                {
                    throw Error.CannotAssignToMember(member.Name);
                }



                Type memberType = TypeSystem.GetMemberType(member);
                Label label = gen.DefineLabel();
                bool flag = HasSideEffect(expr);
                if ((locStoreInMember != null) && !flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                }
                GenerateExpressionForType(expr, memberType, mm.DeclaringType.IsEntity ? locInstance : null);
                LocalBuilder local = gen.DeclareLocal(memberType);
                gen.Emit(OpCodes.Stloc, local);
                if ((locStoreInMember != null) && flag)
                {
                    gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);


                }

                GenerateLoadForMemberAccess(locInstance);

                MethodInfo setItemMethod = null;
                if (member is PropertyInfo)
                {
                    var m = ((PropertyInfo)member).GetSetMethod();
                    if (m == null)
                        m = ((PropertyInfo) member).GetSetMethod(true);

                    if (m.Name == "set_Item")
                        setItemMethod = m;
                }

                if (setItemMethod == null)
                {
                    gen.Emit(OpCodes.Ldloc, local);
                }
                else
                {
                    gen.Emit(OpCodes.Ldstr, member.Name);
                    gen.Emit(OpCodes.Ldloc, local);
                    gen.Emit(OpCodes.Box, local.LocalType);
                }

                GenerateStoreMember(member);

                gen.MarkLabel(label);


            }

            internal void GenerateBody(ILGenerator generator, SqlExpression expression)
            {
                gen = generator;
                globals = new List<object>();
                namedColumns = new List<NamedColumn>();
                locDataReader = generator.DeclareLocal(compiler.dataReaderType);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, compiler.readerField);
                generator.Emit(OpCodes.Stloc, locDataReader);
                GenerateExpressionForType(expression, elementType);
                generator.Emit(OpCodes.Ret);
            }

            private Type GenerateClientArray(SqlClientArray ca)
            {
                Type type = TypeSystem.GetElementType(ca.ClrType);
                GenerateConstInt(ca.Expressions.Count);
                gen.Emit(OpCodes.Newarr, type);
                int num = 0;
                int count = ca.Expressions.Count;
                while (num < count)
                {
                    gen.Emit(OpCodes.Dup);
                    GenerateConstInt(num);
                    GenerateExpressionForType(ca.Expressions[num], type);
                    GenerateArrayAssign(type);
                    num++;
                }
                return ca.ClrType;
            }

            private Type GenerateClientCase(SqlClientCase scc, bool isDeferred, LocalBuilder locInstance)
            {
                LocalBuilder local = gen.DeclareLocal(scc.Expression.ClrType);
                GenerateExpressionForType(scc.Expression, scc.Expression.ClrType);
                gen.Emit(OpCodes.Stloc, local);
                Label loc = gen.DefineLabel();
                Label label = gen.DefineLabel();
                int num = 0;
                int count = scc.Whens.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        gen.MarkLabel(loc);
                        loc = gen.DefineLabel();
                    }
                    SqlClientWhen when = scc.Whens[num];
                    if (when.Match != null)
                    {
                        gen.Emit(OpCodes.Ldloc, local);
                        GenerateExpressionForType(when.Match, scc.Expression.ClrType);
                        GenerateEquals(local.LocalType);
                        gen.Emit(OpCodes.Brfalse, loc);
                    }
                    if (isDeferred)
                    {
                        GenerateDeferredSource(when.Value, locInstance);
                    }
                    else
                    {
                        GenerateExpressionForType(when.Value, scc.ClrType);
                    }
                    gen.Emit(OpCodes.Br, label);
                    num++;
                }
                gen.MarkLabel(label);
                return scc.ClrType;
            }

            private Type GenerateClientParameter(SqlClientParameter cp)
            {
                Delegate delegate2 = cp.Accessor.Compile();
                int iGlobal = AddGlobal(delegate2.GetType(), delegate2);
                GenerateGlobalAccess(iGlobal, delegate2.GetType());
                GenerateAccessArguments();
                MethodInfo meth = delegate2.GetType().GetMethod("Invoke",
                                                                BindingFlags.NonPublic | BindingFlags.Public |
                                                                BindingFlags.Instance, null,
                                                                new[] { typeof(object[]) }, null);
                gen.Emit(GetMethodCallOpCode(meth), meth);
                return delegate2.Method.ReturnType;
            }

            private Type GenerateClientQuery(SqlClientQuery cq, LocalBuilder locInstance)
            {
                Type type = (cq.Query.NodeType == SqlNodeType.Multiset)
                                ? TypeSystem.GetElementType(cq.ClrType)
                                : cq.ClrType;
                gen.Emit(OpCodes.Ldarg_0);
                GenerateConstInt(cq.Ordinal);
                GenerateConstInt(cq.Arguments.Count);
                gen.Emit(OpCodes.Newarr, typeof(object));
                int num = 0;
                int count = cq.Arguments.Count;
                while (num < count)
                {
                    gen.Emit(OpCodes.Dup);
                    GenerateConstInt(num);
                    Type clrType = cq.Arguments[num].ClrType;
                    if (cq.Arguments[num].NodeType == SqlNodeType.ColumnRef)
                    {
                        var ref2 = (SqlColumnRef)cq.Arguments[num];
                        if (clrType.IsValueType && !TypeSystem.IsNullableType(clrType))
                        {
                            clrType = typeof(Nullable<>).MakeGenericType(new[] { clrType });
                        }
                        GenerateColumnAccess(clrType, ref2.SqlType, ref2.Column.Ordinal, null);
                    }
                    else
                    {
                        GenerateExpressionForType(cq.Arguments[num], cq.Arguments[num].ClrType);
                    }
                    if (clrType.IsValueType)
                    {
                        gen.Emit(OpCodes.Box, clrType);
                    }
                    GenerateArrayAssign(typeof(object));
                    num++;
                }
                MethodInfo method =
                    typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }).GetMethod(
                        "ExecuteSubQuery", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                gen.Emit(GetMethodCallOpCode(method), method);
                Type cls = typeof(IEnumerable<>).MakeGenericType(new[] { type });
                gen.Emit(OpCodes.Castclass, cls);
                Type expectedType = typeof(List<>).MakeGenericType(new[] { type });
                GenerateConvertToType(cls, expectedType);
                return expectedType;
            }

            private void GenerateColumnAccess(Type cType, IProviderType pType, int ordinal, LocalBuilder locOrdinal)
            {
                Type closestRuntimeType = pType.GetClosestRuntimeType();
                MethodInfo readerMethod = GetReaderMethod(compiler.dataReaderType, closestRuntimeType);
                MethodInfo meth = GetReaderMethod(typeof(DbDataReader), closestRuntimeType);
                Label label = gen.DefineLabel();
                Label label2 = gen.DefineLabel();
                Label label3 = gen.DefineLabel();
                GenerateAccessBufferReader();
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ceq);
                gen.Emit(OpCodes.Brfalse, label3);
                GenerateAccessDataReader();
                if (locOrdinal != null)
                {
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                }
                else
                {
                    GenerateConstInt(ordinal);
                }
                gen.Emit(GetMethodCallOpCode(compiler.miDRisDBNull), compiler.miDRisDBNull);
                gen.Emit(OpCodes.Brtrue, label);
                GenerateAccessDataReader();
                if (locOrdinal != null)
                {
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                }
                else
                {
                    GenerateConstInt(ordinal);
                }
                gen.Emit(GetMethodCallOpCode(readerMethod), readerMethod);
                GenerateConvertToType(closestRuntimeType, cType, readerMethod.ReturnType);
                gen.Emit(OpCodes.Br_S, label2);
                gen.MarkLabel(label3);
                GenerateAccessBufferReader();
                if (locOrdinal != null)
                {
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                }
                else
                {
                    GenerateConstInt(ordinal);
                }
                gen.Emit(GetMethodCallOpCode(compiler.miBRisDBNull), compiler.miBRisDBNull);
                gen.Emit(OpCodes.Brtrue, label);
                GenerateAccessBufferReader();
                if (locOrdinal != null)
                {
                    gen.Emit(OpCodes.Ldloc, locOrdinal);
                }
                else
                {
                    GenerateConstInt(ordinal);
                }
                gen.Emit(GetMethodCallOpCode(meth), meth);
                GenerateConvertToType(closestRuntimeType, cType, readerMethod.ReturnType);
                gen.Emit(OpCodes.Br_S, label2);
                gen.MarkLabel(label);
                GenerateDefault(cType);
                gen.MarkLabel(label2);
            }

            private Type GenerateColumnReference(SqlColumnRef cref)
            {
                GenerateColumnAccess(cref.ClrType, cref.SqlType, cref.Column.Ordinal, null);
                return cref.ClrType;
            }

            private Type GenerateConstant(Type type, object value)
            {
                if (value == null)
                {
                    if (type.IsValueType)
                    {
                        LocalBuilder local = gen.DeclareLocal(type);
                        gen.Emit(OpCodes.Ldloca, local);
                        gen.Emit(OpCodes.Initobj, type);
                        gen.Emit(OpCodes.Ldloc, local);
                        return type;
                    }
                    gen.Emit(OpCodes.Ldnull);
                    return type;
                }
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        GenerateConstInt(((bool)value) ? 1 : 0);
                        return type;

                    case TypeCode.SByte:
                        GenerateConstInt((sbyte)value);
                        gen.Emit(OpCodes.Conv_I1);
                        return type;

                    case TypeCode.Int16:
                        GenerateConstInt((short)value);
                        gen.Emit(OpCodes.Conv_I2);
                        return type;

                    case TypeCode.Int32:
                        GenerateConstInt((int)value);
                        return type;

                    case TypeCode.Int64:
                        gen.Emit(OpCodes.Ldc_I8, (long)value);
                        return type;

                    case TypeCode.Single:
                        gen.Emit(OpCodes.Ldc_R4, (float)value);
                        return type;

                    case TypeCode.Double:
                        gen.Emit(OpCodes.Ldc_R8, (double)value);
                        return type;
                }
                int iGlobal = AddGlobal(type, value);
                return GenerateGlobalAccess(iGlobal, type);
            }

            private void GenerateConstInt(int value)
            {
                switch (value)
                {
                    case 0:
                        gen.Emit(OpCodes.Ldc_I4_0);
                        return;

                    case 1:
                        gen.Emit(OpCodes.Ldc_I4_1);
                        return;

                    case 2:
                        gen.Emit(OpCodes.Ldc_I4_2);
                        return;

                    case 3:
                        gen.Emit(OpCodes.Ldc_I4_3);
                        return;

                    case 4:
                        gen.Emit(OpCodes.Ldc_I4_4);
                        return;

                    case 5:
                        gen.Emit(OpCodes.Ldc_I4_5);
                        return;

                    case 6:
                        gen.Emit(OpCodes.Ldc_I4_6);
                        return;

                    case 7:
                        gen.Emit(OpCodes.Ldc_I4_7);
                        return;

                    case 8:
                        gen.Emit(OpCodes.Ldc_I4_8);
                        return;

                    case -1:
                        gen.Emit(OpCodes.Ldc_I4_M1);
                        return;
                }
                if ((value >= -127) && (value < 0x80))
                {
                    gen.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                }
                else
                {
                    gen.Emit(OpCodes.Ldc_I4, value);
                }
            }

            private void GenerateConvertToType(Type actualType, Type expectedType)
            {
                if ((expectedType != actualType) && (actualType.IsValueType || !actualType.IsSubclassOf(expectedType)))
                {
                    if (actualType.IsGenericType)
                    {
                        actualType.GetGenericTypeDefinition();
                    }
                    Type type = expectedType.IsGenericType ? expectedType.GetGenericTypeDefinition() : null;
                    Type[] typeArguments = (type != null) ? expectedType.GetGenericArguments() : null;
                    Type elementType = TypeSystem.GetElementType(actualType);
                    Type sequenceType = TypeSystem.GetSequenceType(elementType);
                    bool flag = sequenceType.IsAssignableFrom(actualType);
                    if ((expectedType == typeof(object)) && actualType.IsValueType)
                    {
                        gen.Emit(OpCodes.Box, actualType);
                    }
                    else if ((actualType == typeof(object)) && expectedType.IsValueType)
                    {
                        gen.Emit(OpCodes.Unbox_Any, expectedType);
                    }
                    else if ((actualType.IsSubclassOf(expectedType) || expectedType.IsSubclassOf(actualType)) &&
                             (!actualType.IsValueType && !expectedType.IsValueType))
                    {
                        gen.Emit(OpCodes.Castclass, expectedType);
                    }
                    else if ((type == typeof(IEnumerable<>)) && flag)
                    {
                        if ((this.elementType.IsInterface || typeArguments[0].IsInterface) ||
                            ((this.elementType.IsSubclassOf(typeArguments[0]) ||
                              typeArguments[0].IsSubclassOf(this.elementType)) ||
                             (TypeSystem.GetNonNullableType(this.elementType) ==
                              TypeSystem.GetNonNullableType(typeArguments[0]))))
                        {
                            MethodInfo meth = TypeSystem.FindSequenceMethod("Cast", new[] { sequenceType },
                                                                            new[] { typeArguments[0] });
                            gen.Emit(OpCodes.Call, meth);
                        }
                        else
                        {
                            var t = typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType });
                            MethodInfo info2 = TypeSystem.FindStaticMethod(t, "Convert", new[] { sequenceType },
                                                                           new[] { typeArguments[0] });
                            gen.Emit(OpCodes.Call, info2);
                        }
                    }
                    else if ((expectedType == elementType) && flag)
                    {
                        MethodInfo info3 = TypeSystem.FindSequenceMethod("SingleOrDefault",
                                                                         new[] { sequenceType },
                                                                         new[] { expectedType });
                        gen.Emit(OpCodes.Call, info3);
                    }
                    else if (TypeSystem.IsNullableType(expectedType) &&
                             (TypeSystem.GetNonNullableType(expectedType) == actualType))
                    {
                        ConstructorInfo constructor = expectedType.GetConstructor(new[] { actualType });
                        gen.Emit(OpCodes.Newobj, constructor);
                    }
                    else if (TypeSystem.IsNullableType(actualType) &&
                             (TypeSystem.GetNonNullableType(actualType) == expectedType))
                    {
                        LocalBuilder local = gen.DeclareLocal(actualType);
                        gen.Emit(OpCodes.Stloc, local);
                        gen.Emit(OpCodes.Ldloca, local);
                        GenerateGetValueOrDefault(actualType);
                    }
                    else if ((type == typeof(EntityRef<>)) || (type == typeof(Link<>)))
                    {
                        if (actualType.IsAssignableFrom(typeArguments[0]))
                        {
                            if (actualType != typeArguments[0])
                            {
                                GenerateConvertToType(actualType, typeArguments[0]);
                            }
                            ConstructorInfo con = expectedType.GetConstructor(new[] { typeArguments[0] });
                            gen.Emit(OpCodes.Newobj, con);
                        }
                        else
                        {
                            if (!sequenceType.IsAssignableFrom(actualType))
                            {
                                throw Error.CannotConvertToEntityRef(actualType);
                            }
                            MethodInfo info6 = TypeSystem.FindSequenceMethod("SingleOrDefault",
                                                                             new[] { sequenceType },
                                                                             new[] { elementType });
                            gen.Emit(OpCodes.Call, info6);
                            ConstructorInfo info7 = expectedType.GetConstructor(new[] { elementType });
                            gen.Emit(OpCodes.Newobj, info7);
                        }
                    }
                    else if (((expectedType == typeof(IQueryable)) ||
                              (expectedType == typeof(IOrderedQueryable))) &&
                             typeof(IEnumerable).IsAssignableFrom(actualType))
                    {
                        MethodInfo info8 = TypeSystem.FindQueryableMethod("AsQueryable",
                                                                          new[] { typeof(IEnumerable) },
                                                                          new Type[0]);
                        gen.Emit(OpCodes.Call, info8);
                        if (type == typeof(IOrderedQueryable))
                        {
                            gen.Emit(OpCodes.Castclass, expectedType);
                        }
                    }
                    else if (((type == typeof(IQueryable<>)) || (type == typeof(IOrderedQueryable<>))) &&
                             flag)
                    {
                        if (elementType != typeArguments[0])
                        {
                            sequenceType = typeof(IEnumerable<>).MakeGenericType(typeArguments);
                            GenerateConvertToType(actualType, sequenceType);
                            elementType = typeArguments[0];
                        }
                        MethodInfo info9 = TypeSystem.FindQueryableMethod("AsQueryable",
                                                                          new[] { sequenceType },
                                                                          new[] { elementType });
                        gen.Emit(OpCodes.Call, info9);
                        if (type == typeof(IOrderedQueryable<>))
                        {
                            gen.Emit(OpCodes.Castclass, expectedType);
                        }
                    }
                    else if ((type == typeof(IOrderedEnumerable<>)) && flag)
                    {
                        if (elementType != typeArguments[0])
                        {
                            sequenceType = typeof(IEnumerable<>).MakeGenericType(typeArguments);
                            GenerateConvertToType(actualType, sequenceType);
                            elementType = typeArguments[0];
                        }
                        var t = typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType });
                        MethodInfo info10 = TypeSystem.FindStaticMethod(t, "CreateOrderedEnumerable",
                                                                        new[] { sequenceType }, new[] { elementType });
                        gen.Emit(OpCodes.Call, info10);
                    }
                    else if ((type == typeof(EntitySet<>)) && flag)
                    {
                        if (elementType != typeArguments[0])
                        {
                            sequenceType = typeof(IEnumerable<>).MakeGenericType(typeArguments);
                            GenerateConvertToType(actualType, sequenceType);
                            actualType = sequenceType;
                            elementType = typeArguments[0];
                        }
                        LocalBuilder builder2 = gen.DeclareLocal(actualType);
                        gen.Emit(OpCodes.Stloc, builder2);
                        ConstructorInfo info11 = expectedType.GetConstructor(Type.EmptyTypes);
                        gen.Emit(OpCodes.Newobj, info11);
                        LocalBuilder builder3 = gen.DeclareLocal(expectedType);
                        gen.Emit(OpCodes.Stloc, builder3);
                        gen.Emit(OpCodes.Ldloc, builder3);
                        gen.Emit(OpCodes.Ldloc, builder2);
                        MethodInfo info12 = expectedType.GetMethod("Assign",
                                                                   BindingFlags.NonPublic |
                                                                   BindingFlags.Public |
                                                                   BindingFlags.Instance, null,
                                                                   new[] { sequenceType }, null);
                        gen.Emit(GetMethodCallOpCode(info12), info12);
                        gen.Emit(OpCodes.Ldloc, builder3);
                    }
                    else if ((typeof(IEnumerable).IsAssignableFrom(expectedType) && flag) &&
                             expectedType.IsAssignableFrom(
                                 typeof(List<>).MakeGenericType(new[] { elementType })))
                    {
                        ConstructorInfo info13 =
                            typeof(List<>).MakeGenericType(new[] { elementType }).
                                GetConstructor(new[] { sequenceType });
                        gen.Emit(OpCodes.Newobj, info13);
                    }
                    else if (((expectedType.IsArray && (expectedType.GetArrayRank() == 1)) &&
                              (!actualType.IsArray && sequenceType.IsAssignableFrom(actualType))) &&
                             expectedType.GetElementType().IsAssignableFrom(elementType))
                    {
                        MethodInfo info14 = TypeSystem.FindSequenceMethod("ToArray",
                                                                          new[] { sequenceType },
                                                                          new[] { elementType });
                        gen.Emit(OpCodes.Call, info14);
                    }
                    else
                    {
                        if ((expectedType.IsClass &&
                             typeof(ICollection<>).MakeGenericType(new[] { elementType })
                                 .IsAssignableFrom(expectedType)) &&
                            ((expectedType.GetConstructor(Type.EmptyTypes) != null) &&
                             sequenceType.IsAssignableFrom(actualType)))
                        {
                            throw Error.GeneralCollectionMaterializationNotSupported();
                        }
                        if ((expectedType == typeof(bool)) && (actualType == typeof(int)))
                        {
                            Label label = gen.DefineLabel();
                            Label label2 = gen.DefineLabel();
                            gen.Emit(OpCodes.Ldc_I4_0);
                            gen.Emit(OpCodes.Ceq);
                            gen.Emit(OpCodes.Brtrue_S, label);
                            gen.Emit(OpCodes.Ldc_I4_1);
                            gen.Emit(OpCodes.Br_S, label2);
                            gen.MarkLabel(label);
                            gen.Emit(OpCodes.Ldc_I4_0);
                            gen.MarkLabel(label2);
                        }
                        else
                        {
                            if (actualType.IsValueType)
                                gen.Emit(OpCodes.Box, actualType);

                            gen.Emit(OpCodes.Ldtoken, expectedType);
                            var flags = BindingFlags.Public | BindingFlags.Static;
                            MethodInfo method = typeof(Type).GetMethod("GetTypeFromHandle", flags);
                            gen.Emit(OpCodes.Call, method);
                            flags = BindingFlags.Public | BindingFlags.Static;
                            var info16 = typeof(DBConvert).GetMethod("ChangeType", flags, null,
                                                                     new[] { typeof(object), typeof(Type) }, null);
                            gen.Emit(OpCodes.Call, info16);
                            if (expectedType.IsValueType)
                            {
                                gen.Emit(OpCodes.Unbox_Any, expectedType);
                            }
                            else if (expectedType != typeof(object))
                            {
                                gen.Emit(OpCodes.Castclass, expectedType);
                            }
                        }
                    }
                }
            }

            private void GenerateConvertToType(Type actualType, Type expectedType, Type readerMethodType)
            {
                GenerateConvertToType(actualType, expectedType);
                if (readerMethodType == typeof(object))
                {
                    GenerateConvertToType(typeof(object), expectedType);
                }
            }

            private void GenerateDefault(Type type)
            {
                GenerateDefault(type, true);
            }

            private void GenerateDefault(Type type, bool throwIfNotNullable)
            {
                if (type.IsValueType)
                {
                    if (!throwIfNotNullable || TypeSystem.IsNullableType(type))
                    {
                        LocalBuilder local = gen.DeclareLocal(type);
                        gen.Emit(OpCodes.Ldloca, local);
                        gen.Emit(OpCodes.Initobj, type);
                        gen.Emit(OpCodes.Ldloc, local);
                    }
                    else
                    {
                        gen.Emit(OpCodes.Ldtoken, type);
                        gen.Emit(OpCodes.Call,
                                      typeof(Type).GetMethod("GetTypeFromHandle",
                                                              BindingFlags.Public | BindingFlags.Static));
                        MethodInfo method =
                            typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }).
                                GetMethod("ErrorAssignmentToNull", BindingFlags.Public | BindingFlags.Static);
                        gen.Emit(OpCodes.Call, method);
                        gen.Emit(OpCodes.Throw);
                    }
                }
                else
                {
                    gen.Emit(OpCodes.Ldnull);
                }
            }

            private Type GenerateDeferredSource(SqlExpression expr, LocalBuilder locInstance)
            {
                if (expr.NodeType == SqlNodeType.ClientCase)
                {
                    return GenerateClientCase((SqlClientCase)expr, true, locInstance);
                }
                if (expr.NodeType != SqlNodeType.Link)
                {
                    throw Error.ExpressionNotDeferredQuerySource();
                }
                return GenerateLink((SqlLink)expr, locInstance);
            }

            private Type GenerateDiscriminatedType(SqlDiscriminatedType dt)
            {
                LocalBuilder local = gen.DeclareLocal(dt.Discriminator.ClrType);
                GenerateExpressionForType(dt.Discriminator, dt.Discriminator.ClrType);
                gen.Emit(OpCodes.Stloc, local);
                return GenerateDiscriminatedType(dt.TargetType, local, dt.Discriminator.SqlType);
            }

            private Type GenerateDiscriminatedType(MetaType targetType, LocalBuilder locDiscriminator,
                                                   IProviderType discriminatorType)
            {
                MetaType type = null;
                Label label = gen.DefineLabel();
                Label label2 = gen.DefineLabel();
                foreach (MetaType type2 in targetType.InheritanceTypes)
                {
                    if (type2.InheritanceCode != null)
                    {
                        if (type2.IsInheritanceDefault)
                        {
                            type = type2;
                        }
                        gen.Emit(OpCodes.Ldloc, locDiscriminator);
                        object obj2 = InheritanceRules.InheritanceCodeForClientCompare(type2.InheritanceCode,
                                                                                       discriminatorType);
                        GenerateConstant(locDiscriminator.LocalType, obj2);
                        GenerateEquals(locDiscriminator.LocalType);
                        gen.Emit(OpCodes.Brfalse, label);
                        GenerateConstant(typeof(Type), type2.Type);
                        gen.Emit(OpCodes.Br, label2);
                        gen.MarkLabel(label);
                        label = gen.DefineLabel();
                    }
                }
                gen.MarkLabel(label);
                if (type != null)
                {
                    GenerateConstant(typeof(Type), type.Type);
                }
                else
                {
                    GenerateDefault(typeof(Type));
                }
                gen.MarkLabel(label2);
                return typeof(Type);
            }

            private void GenerateEquals(Type type)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                    case TypeCode.DBNull:
                    case TypeCode.String:
                        {
                            if (type.IsValueType)
                            {
                                LocalBuilder local = gen.DeclareLocal(type);
                                LocalBuilder builder2 = gen.DeclareLocal(type);
                                gen.Emit(OpCodes.Stloc, builder2);
                                gen.Emit(OpCodes.Stloc, local);
                                gen.Emit(OpCodes.Ldloc, local);
                                gen.Emit(OpCodes.Box, type);
                                gen.Emit(OpCodes.Ldloc, builder2);
                                gen.Emit(OpCodes.Box, type);
                            }
                            MethodInfo method = typeof(object).GetMethod("Equals",
                                                                          BindingFlags.Public | BindingFlags.Static);
                            gen.Emit(GetMethodCallOpCode(method), method);
                            return;
                        }
                }
                gen.Emit(OpCodes.Ceq);
            }

            private Type GenerateExpressionForType(SqlExpression expr, Type type)
            {
                return GenerateExpressionForType(expr, type, null);
            }

            private Type GenerateExpressionForType(SqlExpression expr, Type type, LocalBuilder locInstance)
            {
                Type actualType = Generate(expr, locInstance);
                GenerateConvertToType(actualType, type);
                return type;
            }

            private void GenerateGetValue(Type nullableType)
            {
                MethodInfo method = nullableType.GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance);
                gen.Emit(OpCodes.Call, method);
            }

            private void GenerateGetValueOrDefault(Type nullableType)
            {
                MethodInfo method = nullableType.GetMethod("GetValueOrDefault", Type.EmptyTypes);
                gen.Emit(OpCodes.Call, method);
            }

            private Type GenerateGlobalAccess(int iGlobal, Type type)
            {
                GenerateAccessGlobals();
                if (type.IsValueType)
                {
                    GenerateConstInt(iGlobal);
                    gen.Emit(OpCodes.Ldelem_Ref);
                    Type cls = typeof(StrongBox<>).MakeGenericType(new[] { type });
                    gen.Emit(OpCodes.Castclass, cls);
                    FieldInfo field = cls.GetField("Value",
                                                   BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                                   BindingFlags.DeclaredOnly);
                    gen.Emit(OpCodes.Ldfld, field);
                    return type;
                }
                GenerateConstInt(iGlobal);
                gen.Emit(OpCodes.Ldelem_Ref);
                GenerateConvertToType(typeof(object), type);
                gen.Emit(OpCodes.Castclass, type);
                return type;
            }

            private Type GenerateGrouping(SqlGrouping grp)
            {
                Type[] genericArguments = grp.ClrType.GetGenericArguments();
                GenerateExpressionForType(grp.Key, genericArguments[0]);
                Generate(grp.Group);
                MethodInfo meth =
                    TypeSystem.FindStaticMethod(
                        typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }),
                        "CreateGroup",
                        new[]
                            {
                                genericArguments[0],
                                typeof (IEnumerable<>).MakeGenericType(new[] {genericArguments[1]})
                            }, genericArguments);
                gen.Emit(OpCodes.Call, meth);
                return meth.ReturnType;
            }

            private void GenerateHasValue(Type nullableType)
            {
                MethodInfo method = nullableType.GetMethod("get_HasValue", BindingFlags.Public | BindingFlags.Instance);
                gen.Emit(OpCodes.Call, method);
            }

            private Type GenerateJoinedCollection(SqlJoinedCollection jc)
            {
                LocalBuilder local = gen.DeclareLocal(typeof(int));
                LocalBuilder builder2 = gen.DeclareLocal(typeof(bool));
                Type clrType = jc.Expression.ClrType;
                Type localType = typeof(List<>).MakeGenericType(new[] { clrType });
                LocalBuilder builder3 = gen.DeclareLocal(localType);
                GenerateExpressionForType(jc.Count, typeof(int));
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloc, local);
                ConstructorInfo constructor = localType.GetConstructor(new[] { typeof(int) });
                gen.Emit(OpCodes.Newobj, constructor);
                gen.Emit(OpCodes.Stloc, builder3);
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Stloc, builder2);
                Label label = gen.DefineLabel();
                Label loc = gen.DefineLabel();
                LocalBuilder builder4 = gen.DeclareLocal(typeof(int));
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Stloc, builder4);
                gen.Emit(OpCodes.Br, label);
                gen.MarkLabel(loc);
                gen.Emit(OpCodes.Ldloc, builder4);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Cgt);
                gen.Emit(OpCodes.Ldloc, builder2);
                gen.Emit(OpCodes.And);
                Label label3 = gen.DefineLabel();
                gen.Emit(OpCodes.Brfalse, label3);
                gen.Emit(OpCodes.Ldarg_0);
                MethodInfo meth =
                    typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }).GetMethod(
                        "Read", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                        Type.EmptyTypes, null);
                gen.Emit(GetMethodCallOpCode(meth), meth);
                gen.Emit(OpCodes.Stloc, builder2);
                gen.MarkLabel(label3);
                Label label4 = gen.DefineLabel();
                gen.Emit(OpCodes.Ldloc, builder2);
                gen.Emit(OpCodes.Brfalse, label4);
                gen.Emit(OpCodes.Ldloc, builder3);
                GenerateExpressionForType(jc.Expression, clrType);
                MethodInfo info3 = localType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null,
                                                       new[] { clrType }, null);
                gen.Emit(GetMethodCallOpCode(info3), info3);
                gen.MarkLabel(label4);
                gen.Emit(OpCodes.Ldloc, builder4);
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Add);
                gen.Emit(OpCodes.Stloc, builder4);
                gen.MarkLabel(label);
                gen.Emit(OpCodes.Ldloc, builder4);
                gen.Emit(OpCodes.Ldloc, local);
                gen.Emit(OpCodes.Clt);
                gen.Emit(OpCodes.Ldloc, builder2);
                gen.Emit(OpCodes.And);
                gen.Emit(OpCodes.Brtrue, loc);
                gen.Emit(OpCodes.Ldloc, builder3);
                return localType;
            }

            private Type GenerateLift(SqlLift lift)
            {
                return GenerateExpressionForType(lift.Expression, lift.ClrType);
            }

            private Type GenerateLink(SqlLink link, LocalBuilder locInstance)
            {
                gen.Emit(OpCodes.Ldarg_0);
                int num = AddGlobal(typeof(MetaDataMember), link.Member);
                GenerateConstInt(num);
                int num2 = AllocateLocal();
                GenerateConstInt(num2);
                Type type = (link.Member.IsAssociation && link.Member.Association.IsMany)
                                ? TypeSystem.GetElementType(link.Member.Type)
                                : link.Member.Type;
                if (locInstance != null)
                {
                    gen.Emit(OpCodes.Ldloc, locInstance);
                    MethodInfo meth =
                        typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }).
                            GetMethod("GetNestedLinkSource",
                                      BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).
                            MakeGenericMethod(new[] { type });
                    gen.Emit(GetMethodCallOpCode(meth), meth);
                }
                else
                {
                    GenerateConstInt(link.KeyExpressions.Count);
                    gen.Emit(OpCodes.Newarr, typeof(object));
                    int num3 = 0;
                    int count = link.KeyExpressions.Count;
                    while (num3 < count)
                    {
                        gen.Emit(OpCodes.Dup);
                        GenerateConstInt(num3);
                        GenerateExpressionForType(link.KeyExpressions[num3], typeof(object));
                        GenerateArrayAssign(typeof(object));
                        num3++;
                    }
                    MethodInfo info3 =
                        typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }).
                            GetMethod("GetLinkSource",
                                      BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).
                            MakeGenericMethod(new[] { type });
                    gen.Emit(GetMethodCallOpCode(info3), info3);
                }
                return typeof(IEnumerable<>).MakeGenericType(new[] { type });
            }

            private void GenerateLoadForMemberAccess(LocalBuilder loc)
            {

                Debug.Assert(loc.LocalType != null);

                if (loc.LocalType.IsValueType)
                {
                    gen.Emit(OpCodes.Ldloca, loc);
                }
                else
                {
                    gen.Emit(OpCodes.Ldloc, loc);
                }

            }

            private void GenerateLoadMember(MemberInfo mi)
            {
                var field = mi as FieldInfo;
                if (field != null)
                {
                    gen.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    var getMethod = ((PropertyInfo)mi).GetGetMethod(true);
                    gen.Emit(GetMethodCallOpCode(getMethod), getMethod);
                }
            }

            private Type GenerateMember(SqlMember m)
            {
                var member = m.Member as FieldInfo;
                if (member != null)
                {
                    GenerateExpressionForType(m.Expression, m.Expression.ClrType);
                    gen.Emit(OpCodes.Ldfld, member);
                    return member.FieldType;
                }
                //if(m.Member is MethodInfo)

                MethodInfo methodInfo;
                if (m.Member is MethodInfo)
                    methodInfo = (MethodInfo)m.Member;
                else
                    methodInfo = ((PropertyInfo)m.Member).GetGetMethod();

                return GenerateMethodCall(new SqlMethodCall(m.ClrType, m.SqlType, methodInfo,
                                                            m.Expression, null, m.SourceExpression));
            }

            private void GenerateMemberAssignment(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr,
                                                  LocalBuilder locStoreInMember)
            {
                var bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
                MemberInfo mi = mm.StorageMember ?? mm.Member;
                Type memberType = TypeSystem.GetMemberType(mi);
                if (IsDeferrableExpression(expr) && ((compiler.services.Context.LoadOptions == null) ||
                    !compiler.services.Context.LoadOptions.IsPreloaded(mm.Member)))
                {
                    if (mm.IsDeferred)
                    {
                        gen.Emit(OpCodes.Ldarg_0);
                        MethodInfo getMethod = typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }).
                                                                            GetProperty("CanDeferLoad").GetGetMethod();
                        gen.Emit(GetMethodCallOpCode(getMethod), getMethod);
                        Label label = gen.DefineLabel();
                        gen.Emit(OpCodes.Brfalse, label);
                        if (!memberType.IsGenericType)
                        {
                            throw Error.DeferredMemberWrongType();
                        }
                        Type genericTypeDefinition = memberType.GetGenericTypeDefinition();
                        if (genericTypeDefinition == typeof(EntitySet<>))
                        {
                            GenerateAssignDeferredEntitySet(mm, locInstance, expr, locStoreInMember);
                        }
                        else
                        {
                            if ((genericTypeDefinition != typeof(EntityRef<>)) &&
                                (genericTypeDefinition != typeof(Link<>)))
                            {
                                throw Error.DeferredMemberWrongType();
                            }
                            GenerateAssignDeferredReference(mm, locInstance, expr, locStoreInMember);
                        }
                        gen.MarkLabel(label);
                    }
                }
                else if (memberType.IsGenericType && (memberType.GetGenericTypeDefinition() == typeof(EntitySet<>)))
                {
                    GenerateAssignEntitySet(mm, locInstance, expr, locStoreInMember);
                }
                else
                {
                    GenerateAssignValue(mm, locInstance, expr, locStoreInMember);
                }
            }

            private Type GenerateMethodCall(SqlMethodCall mc)
            {
                ParameterInfo[] parameters = mc.Method.GetParameters();
                if (mc.Object != null)
                {
                    Type localType = GenerateExpressionForType(mc.Object, mc.Object.ClrType);
                    if (localType.IsValueType)
                    {
                        LocalBuilder local = gen.DeclareLocal(localType);
                        gen.Emit(OpCodes.Stloc, local);
                        gen.Emit(OpCodes.Ldloca, local);
                    }
                }
                int index = 0;
                int count = mc.Arguments.Count;
                while (index < count)
                {
                    ParameterInfo info = parameters[index];
                    Type parameterType = info.ParameterType;
                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                        GenerateExpressionForType(mc.Arguments[index], parameterType);
                        LocalBuilder builder2 = gen.DeclareLocal(parameterType);
                        gen.Emit(OpCodes.Stloc, builder2);
                        gen.Emit(OpCodes.Ldloca, builder2);
                    }
                    else
                    {
                        GenerateExpressionForType(mc.Arguments[index], parameterType);
                    }
                    index++;
                }
                OpCode methodCallOpCode = GetMethodCallOpCode(mc.Method);
                if (((mc.Object != null) && TypeSystem.IsNullableType(mc.Object.ClrType)) &&
                    (methodCallOpCode == OpCodes.Callvirt))
                {
                    gen.Emit(OpCodes.Constrained, mc.Object.ClrType);
                }
                gen.Emit(methodCallOpCode, mc.Method);
                return mc.Method.ReturnType;
            }

            private Type GenerateNew(SqlNew sn)
            {
                LocalBuilder local = gen.DeclareLocal(sn.ClrType);
                LocalBuilder builder = null;
                Label label = gen.DefineLabel();
                Label label2 = gen.DefineLabel();
                if (sn.Args.Count > 0)
                {
                    ParameterInfo[] parameters = sn.Constructor.GetParameters();
                    int index = 0;
                    int count = sn.Args.Count;
                    while (index < count)
                    {
                        GenerateExpressionForType(sn.Args[index], parameters[index].ParameterType);
                        index++;
                    }
                }
                if (sn.Constructor != null)
                {
                    gen.Emit(OpCodes.Newobj, sn.Constructor);
                    gen.Emit(OpCodes.Stloc, local);
                }
                else if (sn.ClrType.IsValueType)
                {
                    gen.Emit(OpCodes.Ldloca, local);
                    gen.Emit(OpCodes.Initobj, sn.ClrType);
                }
                else
                {
                    ConstructorInfo constructor = sn.ClrType.GetConstructor(Type.EmptyTypes);
                    gen.Emit(OpCodes.Newobj, constructor);
                    gen.Emit(OpCodes.Stloc, local);
                }
                foreach (var assign in sn.Members.OrderBy(m => sn.MetaType.GetDataMember(m.Member).Ordinal))
                {
                    MetaDataMember dataMember = sn.MetaType.GetDataMember(assign.Member);
                    if (dataMember.IsPrimaryKey)
                    {
                        GenerateMemberAssignment(dataMember, local, assign.Expression, null);
                    }
                }
                int num3 = 0;
                if (sn.MetaType.IsEntity)
                {
                    LocalBuilder builder3 = gen.DeclareLocal(sn.ClrType);
                    builder = gen.DeclareLocal(typeof(bool));
                    Label label3 = gen.DefineLabel();
                    num3 = AddGlobal(typeof(MetaType), sn.MetaType);
                    Type type = typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType });
                    gen.Emit(OpCodes.Ldarg_0);
                    GenerateConstInt(num3);
                    gen.Emit(OpCodes.Ldloc, local);
                    MethodInfo meth = type.GetMethod("InsertLookup",
                                                     BindingFlags.NonPublic | BindingFlags.Public |
                                                     BindingFlags.Instance, null,
                                                     new[] { typeof(int), typeof(object) }, null);
                    gen.Emit(GetMethodCallOpCode(meth), meth);
                    gen.Emit(OpCodes.Castclass, sn.ClrType);
                    gen.Emit(OpCodes.Stloc, builder3);
                    gen.Emit(OpCodes.Ldloc, builder3);
                    gen.Emit(OpCodes.Ldloc, local);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brfalse, label2);
                    GenerateConstInt(1);
                    gen.Emit(OpCodes.Stloc, builder);
                    gen.Emit(OpCodes.Br_S, label3);
                    gen.MarkLabel(label2);
                    gen.Emit(OpCodes.Ldloc, builder3);
                    gen.Emit(OpCodes.Stloc, local);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Stloc, builder);
                    gen.MarkLabel(label3);
                }
                foreach (var assign2 in sn.Members.OrderBy(m => sn.MetaType.GetDataMember(m.Member).Ordinal))
                {
                    MetaDataMember mm = sn.MetaType.GetDataMember(assign2.Member);
                    if (!mm.IsPrimaryKey)
                    {
                        GenerateMemberAssignment(mm, local, assign2.Expression, builder);
                    }
                }
                if (sn.MetaType.IsEntity)
                {
                    gen.Emit(OpCodes.Ldloc, builder);
                    GenerateConstInt(0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Brtrue, label);
                    gen.Emit(OpCodes.Ldarg_0);
                    GenerateConstInt(num3);
                    gen.Emit(OpCodes.Ldloc, local);
                    MethodInfo info3 =
                        typeof(ObjectMaterializer<>).MakeGenericType(new[] { compiler.dataReaderType }).
                            GetMethod("SendEntityMaterialized",
                                      BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                                      new[] { typeof(int), typeof(object) }, null);
                    gen.Emit(GetMethodCallOpCode(info3), info3);
                }
                gen.MarkLabel(label);
                gen.Emit(OpCodes.Ldloc, local);
                return sn.ClrType;
            }

            private Type GenerateOptionalValue(SqlOptionalValue opt)
            {
                Label label = gen.DefineLabel();
                Label label2 = gen.DefineLabel();
                Type localType = Generate(opt.HasValue);
                LocalBuilder local = gen.DeclareLocal(localType);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloca, local);
                GenerateHasValue(localType);
                gen.Emit(OpCodes.Brfalse, label);
                GenerateExpressionForType(opt.Value, opt.ClrType);
                gen.Emit(OpCodes.Br_S, label2);
                gen.MarkLabel(label);
                GenerateConstant(opt.ClrType, null);
                gen.MarkLabel(label2);
                return opt.ClrType;
            }

            private Type GenerateSearchedCase(SqlSearchedCase ssc)
            {
                Label loc = gen.DefineLabel();
                Label label = gen.DefineLabel();
                int num = 0;
                int count = ssc.Whens.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        gen.MarkLabel(loc);
                        loc = gen.DefineLabel();
                    }
                    SqlWhen when = ssc.Whens[num];
                    if (when.Match != null)
                    {
                        GenerateExpressionForType(when.Match, typeof(bool));
                        GenerateConstInt(0);
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Brtrue, loc);
                    }
                    GenerateExpressionForType(when.Value, ssc.ClrType);
                    gen.Emit(OpCodes.Br, label);
                    num++;
                }
                gen.MarkLabel(loc);
                if (ssc.Else != null)
                {
                    GenerateExpressionForType(ssc.Else, ssc.ClrType);
                }
                gen.MarkLabel(label);
                return ssc.ClrType;
            }

            private void GenerateStoreMember(MemberInfo mi)
            {
                var field = mi as FieldInfo;
                if (field != null)
                {
                    gen.Emit(OpCodes.Stfld, field);
                }
                else
                {
                    MethodInfo setMethod = ((PropertyInfo)mi).GetSetMethod(true);
                    gen.Emit(GetMethodCallOpCode(setMethod), setMethod);
                }
            }

            private Type GenerateTypeCase(SqlTypeCase stc)
            {
                LocalBuilder local = gen.DeclareLocal(stc.Discriminator.ClrType);
                GenerateExpressionForType(stc.Discriminator, stc.Discriminator.ClrType);
                gen.Emit(OpCodes.Stloc, local);
                Label loc = gen.DefineLabel();
                Label label = gen.DefineLabel();
                bool flag = false;
                int num = 0;
                int count = stc.Whens.Count;
                while (num < count)
                {
                    if (num > 0)
                    {
                        gen.MarkLabel(loc);
                        loc = gen.DefineLabel();
                    }
                    SqlTypeCaseWhen when = stc.Whens[num];
                    if (when.Match != null)
                    {
                        gen.Emit(OpCodes.Ldloc, local);
                        var match = when.Match as SqlValue;
                        GenerateConstant(local.LocalType, match.Value);
                        GenerateEquals(local.LocalType);
                        gen.Emit(OpCodes.Brfalse, loc);
                    }
                    else
                    {
                        flag = true;
                    }
                    GenerateExpressionForType(when.TypeBinding, stc.ClrType);
                    gen.Emit(OpCodes.Br, label);
                    num++;
                }
                gen.MarkLabel(loc);
                if (!flag)
                {
                    GenerateConstant(stc.ClrType, null);
                }
                gen.MarkLabel(label);
                return stc.ClrType;
            }

            private Type GenerateUserColumn(SqlUserColumn suc)
            {
                if (string.IsNullOrEmpty(suc.Name))
                {
                    GenerateColumnAccess(suc.ClrType, suc.SqlType, 0, null);
                    return suc.ClrType;
                }
                int count = namedColumns.Count;
                namedColumns.Add(new NamedColumn(suc.Name, suc.IsRequired));
                Label label = gen.DefineLabel();
                Label label2 = gen.DefineLabel();
                LocalBuilder local = gen.DeclareLocal(typeof(int));
                GenerateAccessOrdinals();
                GenerateConstInt(count);
                GenerateArrayAccess(typeof(int), false);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloc, local);
                GenerateConstInt(0);
                gen.Emit(OpCodes.Clt);
                gen.Emit(OpCodes.Brtrue, label);
                GenerateColumnAccess(suc.ClrType, suc.SqlType, 0, local);
                gen.Emit(OpCodes.Br_S, label2);
                gen.MarkLabel(label);
                GenerateDefault(suc.ClrType, false);
                gen.MarkLabel(label2);
                return suc.ClrType;
            }

            private Type GenerateValue(SqlValue value)
            {
                return GenerateConstant(value.ClrType, value.Value);
            }

            private Type GenerateValueOf(SqlUnary u)
            {
                GenerateExpressionForType(u.Operand, u.Operand.ClrType);
                LocalBuilder local = gen.DeclareLocal(u.Operand.ClrType);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloca, local);
                GenerateGetValue(u.Operand.ClrType);
                return u.ClrType;
            }

            private static OpCode GetMethodCallOpCode(MethodInfo mi)
            {
                if (!mi.IsStatic && !mi.DeclaringType.IsValueType)
                {
                    return OpCodes.Callvirt;
                }
                return OpCodes.Call;
            }

            private MethodInfo GetReaderMethod(Type readerType, Type valueType)
            {
                string str;
                if (valueType.IsEnum)
                {
                    valueType = valueType.BaseType;
                }
                if (Type.GetTypeCode(valueType) == TypeCode.Single)
                {
                    str = "GetFloat";
                }
                //else if (valueType != typeof(Guid))
                //{
                //    str = "GetValue";
                //}
                else
                {
                    str = "Get" + valueType.Name;
                }
                MethodInfo info = readerType.GetMethod(str, BindingFlags.Public | BindingFlags.Instance, null,
                                                       readMethodSignature, null);
                if (info == null)
                {
                    info = readerType.GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, null,
                                                readMethodSignature, null);
                }
                return info;
            }

            private bool HasSideEffect(SqlNode node)
            {
                return sideEffectChecker.HasSideEffect(node);
            }

            private static bool IsAssignable(MemberInfo member)
            {
                if (member is FieldInfo)
                {
                    return true;
                }
                var info2 = member as PropertyInfo;
                return ((info2 != null) && info2.CanWrite);
            }

            private bool IsDeferrableExpression(SqlExpression expr)
            {
                if (expr.NodeType != SqlNodeType.Link)
                {
                    if (expr.NodeType != SqlNodeType.ClientCase)
                    {
                        return false;
                    }
                    SqlClientCase @case = (SqlClientCase)expr;
                    foreach (SqlClientWhen when in @case.Whens)
                    {
                        if (!IsDeferrableExpression(when.Value))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            // Properties
            internal object[] Globals
            {
                get { return globals.ToArray(); }
            }

            internal int Locals { get; private set; }

            internal NamedColumn[] NamedColumns
            {
                get { return namedColumns.ToArray(); }
            }
        }
    }
}
