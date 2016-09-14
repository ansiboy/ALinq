using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;
using System;

namespace ALinq.Mapping
{
    internal sealed class MappedRootType : MappedType
    {
        // Fields
        private readonly Dictionary<Type, MetaType> derivedTypes;
        private readonly bool hasInheritance;
        private readonly Dictionary<object, MetaType> inheritanceCodes;
        private MetaType inheritanceDefault;
        private readonly ReadOnlyCollection<MetaType> inheritanceTypes;

        // Methods
        public MappedRootType(MetaModel model, MetaTable table, TypeMapping typeMapping, Type type)
            : base(model, table, typeMapping, type, null)
        {
            if (typeMapping == null)
            {
                throw Error.ArgumentNull("typeMapping");
            }
            if ((typeMapping.InheritanceCode != null) || (typeMapping.DerivedTypes.Count > 0))
            {
                if (Discriminator == null)
                {
                    throw Error.NoDiscriminatorFound(type.Name);
                }
                hasInheritance = true;
                if (!MappingSystem.IsSupportedDiscriminatorType(Discriminator.Type))
                {
                    throw Error.DiscriminatorClrTypeNotSupported(Discriminator.DeclaringType.Name, Discriminator.Name, Discriminator.Type);
                }
                derivedTypes = new Dictionary<Type, MetaType>();
                inheritanceCodes = new Dictionary<object, MetaType>();
                InitInheritedType(typeMapping, this);
            }
            if ((inheritanceDefault == null) && ((inheritanceCode != null) || ((inheritanceCodes != null) && (inheritanceCodes.Count > 0))))
            {
                throw Error.InheritanceHierarchyDoesNotDefineDefault(type);
            }
            if (derivedTypes != null)
            {
                inheritanceTypes = derivedTypes.Values.ToList().AsReadOnly();
            }
            else
            {
                inheritanceTypes = new MetaType[] { this }.ToList().AsReadOnly();
            }
            Validate();
        }

        public override MetaType GetInheritanceType(Type type)
        {
            if (type == Type)
            {
                return this;
            }
            MetaType type2 = null;
            if (derivedTypes != null)
            {
                derivedTypes.TryGetValue(type, out type2);
            }
            return type2;
        }

        private MetaType InitDerivedTypes(TypeMapping typeMap)
        {
            Type type = ((MappedMetaModel)Model).FindType(typeMap.Name);
            if (type == null)
            {
                throw Error.CouldNotFindRuntimeTypeForMapping(typeMap.Name);
            }
            var type2 = new MappedType(Model, Table, typeMap, type, this);
            return InitInheritedType(typeMap, type2);
        }

        private MetaType InitInheritedType(TypeMapping typeMap, MappedType type)
        {
            derivedTypes.Add(type.Type, type);
            if (typeMap.InheritanceCode != null)
            {
                if (Discriminator == null)
                {
                    throw Error.NoDiscriminatorFound(type.Name);
                }
                if (type.Type.IsAbstract)
                {
                    throw Error.AbstractClassAssignInheritanceDiscriminator(type.Type);
                }
                object objB = DBConvert.ChangeType(typeMap.InheritanceCode, Discriminator.Type);
                foreach (object obj3 in inheritanceCodes.Keys)
                {
                    if ((((objB.GetType() == typeof(string)) && (((string)objB).Trim().Length == 0)) && ((obj3.GetType() == typeof(string)) && (((string)obj3).Trim().Length == 0))) || Equals(obj3, objB))
                    {
                        throw Error.InheritanceCodeUsedForMultipleTypes(objB);
                    }
                }
                if (type.inheritanceCode != null)
                {
                    throw Error.InheritanceTypeHasMultipleDiscriminators(type);
                }
                type.inheritanceCode = objB;
                inheritanceCodes.Add(objB, type);
                if (typeMap.IsInheritanceDefault)
                {
                    if (inheritanceDefault != null)
                    {
                        throw Error.InheritanceTypeHasMultipleDefaults(type);
                    }
                    inheritanceDefault = type;
                }
            }
            foreach (TypeMapping mapping in typeMap.DerivedTypes)
            {
                InitDerivedTypes(mapping);
            }
            return type;
        }

        private void Validate()
        {
            var dictionary = new Dictionary<object, string>();
            foreach (MetaType type in InheritanceTypes)
            {
                foreach (MetaDataMember member in type.PersistentDataMembers)
                {
                    if (member.IsDeclaredBy(type))
                    {
                        if (member.IsDiscriminator && !HasInheritance)
                        {
                            throw Error.NonInheritanceClassHasDiscriminator(type);
                        }
                        if (!member.IsAssociation && !string.IsNullOrEmpty(member.MappedName))
                        {
                            string str;
                            object key = InheritanceRules.DistinguishedMemberName(member.Member);
                            if (dictionary.TryGetValue(key, out str))
                            {
                                if (str != member.MappedName)
                                {
                                    throw Error.MemberMappedMoreThanOnce(member.Member.Name);
                                }
                            }
                            else
                            {
                                dictionary.Add(key, member.MappedName);
                            }
                        }
                    }
                }
            }
        }

        // Properties
        public override bool HasInheritance
        {
            get
            {
                return hasInheritance;
            }
        }

        public override bool HasInheritanceCode
        {
            get
            {
                return (InheritanceCode != null);
            }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                return inheritanceDefault;
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                return inheritanceTypes;
            }
        }
    }

    internal static class MethodFinder
    {
        // Methods
        internal static MethodInfo FindMethod(Type type, string name, BindingFlags flags, Type[] argTypes)
        {
            return FindMethod(type, name, flags, argTypes, true);
        }

        internal static MethodInfo FindMethod(Type type, string name, BindingFlags flags, Type[] argTypes, bool allowInherit)
        {
            while (type != typeof(object))
            {
                MethodInfo info = type.GetMethod(name, flags | BindingFlags.DeclaredOnly, null, argTypes, null);
                if ((info != null) || !allowInherit)
                {
                    return info;
                }
                type = type.BaseType;
            }
            return null;
        }
    }

 

  internal sealed class MappedTable : MetaTable
{
    // Fields
    private MethodInfo deleteMethod;
    private bool hasMethods;
    private MethodInfo insertMethod;
    private TableMapping mapping;
    private MappedMetaModel model;
    private readonly MetaType rowType;
    private MethodInfo updateMethod;

    // Methods
    internal MappedTable(MappedMetaModel model, TableMapping mapping, Type rowType)
    {
        this.model = model;
        this.mapping = mapping;
        this.rowType = new MappedRootType(model, this, mapping.RowType, rowType);
    }

    private void InitMethods()
    {
        if (!hasMethods)
        {
            insertMethod = MethodFinder.FindMethod(model.ContextType, "Insert" + rowType.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new[] { rowType.Type });
            updateMethod = MethodFinder.FindMethod(model.ContextType, "Update" + rowType.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new[] { rowType.Type });
            deleteMethod = MethodFinder.FindMethod(model.ContextType, "Delete" + rowType.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new[] { rowType.Type });
            hasMethods = true;
        }
    }

    // Properties
    public override MethodInfo DeleteMethod
    {
        get
        {
            InitMethods();
            return deleteMethod;
        }
    }

    public override MethodInfo InsertMethod
    {
        get
        {
            InitMethods();
            return insertMethod;
        }
    }

    public override MetaModel Model
    {
        [System.Diagnostics.DebuggerStepThrough]
        get
        {
            return model;
        }
    }

    public override MetaType RowType
    {
        [System.Diagnostics.DebuggerStepThrough]
        get
        {
            return rowType;
        }
    }

    public override string TableName
    {
        [System.Diagnostics.DebuggerStepThrough]
        get
        {
            return mapping.TableName;
        }
    }

    public override MethodInfo UpdateMethod
    {
        get
        {
            InitMethods();
            return updateMethod;
        }
    }
}

 

}