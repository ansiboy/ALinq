using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal sealed class AttributedRootType : AttributedMetaType
    {
        // Fields
        private readonly Dictionary<object, MetaType> codeMap;
        private readonly MetaType inheritanceDefault;
        private readonly ReadOnlyCollection<MetaType> inheritanceTypes;
        private readonly Dictionary<Type, MetaType> types;
        private AttributedMetaModel model;

        // Methods
        internal AttributedRootType(AttributedMetaModel model, AttributedMetaTable table, Type type)
            : base(model, table, type, null)
        {
            this.model = model;
            var customAttributes = model.AttributeProvider.GetInheritanceMappingAttribute(type);
            //(InheritanceMappingAttribute[])type.GetCustomAttributes(typeof(InheritanceMappingAttribute), true);
            if (customAttributes != null && customAttributes.Length > 0)
            {
                if (this.Discriminator == null)
                {
                    throw Mapping.Error.NoDiscriminatorFound(type);
                }
                if (!MappingSystem.IsSupportedDiscriminatorType(this.Discriminator.Type))
                {
                    throw Mapping.Error.DiscriminatorClrTypeNotSupported(this.Discriminator.DeclaringType.Name, this.Discriminator.Name, this.Discriminator.Type);
                }
                this.types = new Dictionary<Type, MetaType>();
                this.types.Add(type, this);
                this.codeMap = new Dictionary<object, MetaType>();
                foreach (InheritanceMappingAttribute attribute in customAttributes)
                {
                    if (!type.IsAssignableFrom(attribute.Type))
                    {
                        throw Mapping.Error.InheritanceTypeDoesNotDeriveFromRoot(attribute.Type, type);
                    }
                    if (attribute.Type.IsAbstract)
                    {
                        throw Mapping.Error.AbstractClassAssignInheritanceDiscriminator(attribute.Type);
                    }
                    AttributedMetaType type2 = this.CreateInheritedType(type, attribute.Type);
                    if (attribute.Code == null)
                    {
                        throw Mapping.Error.InheritanceCodeMayNotBeNull();
                    }
                    if (type2.inheritanceCode != null)
                    {
                        throw Mapping.Error.InheritanceTypeHasMultipleDiscriminators(attribute.Type);
                    }
                    object objB = DBConvert.ChangeType(attribute.Code, this.Discriminator.Type);
                    foreach (object obj3 in this.codeMap.Keys)
                    {
                        if ((((objB.GetType() == typeof(string)) && (((string)objB).Trim().Length == 0)) && ((obj3.GetType() == typeof(string)) && (((string)obj3).Trim().Length == 0))) || object.Equals(obj3, objB))
                        {
                            throw Mapping.Error.InheritanceCodeUsedForMultipleTypes(objB);
                        }
                    }
                    type2.inheritanceCode = objB;
                    this.codeMap.Add(objB, type2);
                    if (attribute.IsDefault)
                    {
                        if (this.inheritanceDefault != null)
                        {
                            throw Mapping.Error.InheritanceTypeHasMultipleDefaults(type);
                        }
                        this.inheritanceDefault = type2;
                    }
                }
                if (this.inheritanceDefault == null)
                {
                    throw Mapping.Error.InheritanceHierarchyDoesNotDefineDefault(type);
                }
            }
            if (this.types != null)
            {
                this.inheritanceTypes = this.types.Values.ToList<MetaType>().AsReadOnly();
            }
            else
            {
                this.inheritanceTypes = new MetaType[] { this }.ToList<MetaType>().AsReadOnly();
            }
            this.Validate();
        }

        private AttributedMetaType CreateInheritedType(Type root, Type type)
        {
            MetaType type2;
            if (!this.types.TryGetValue(type, out type2))
            {
                Debug.Assert(model != null);
                type2 = new AttributedMetaType(this.model, this.Table, type, this);
                this.types.Add(type, type2);
                if ((type != root) && (type.BaseType != typeof(object)))
                {
                    this.CreateInheritedType(root, type.BaseType);
                }
            }
            return (AttributedMetaType)type2;
        }

        public override MetaType GetInheritanceType(Type type)
        {
            if (type == this.Type)
            {
                return this;
            }
            MetaType type2 = null;
            if (this.types != null)
            {
                this.types.TryGetValue(type, out type2);
            }
            return type2;
        }

        private void Validate()
        {
            Dictionary<object, string> dictionary = new Dictionary<object, string>();
            foreach (MetaType type in this.InheritanceTypes)
            {
                if (type != this)
                {
                    /*
                    TableAttribute[] customAttributes = (TableAttribute[])type.Type.GetCustomAttributes(typeof(TableAttribute), false);
                    if (customAttributes.Length > 0)
                    {
                        throw Mapping.Error.InheritanceSubTypeIsAlsoRoot(type.Type);
                    }
                    */
                    var tableAttribute = model.AttributeProvider.GetTableAttribute(type.Type);
                    if (tableAttribute != null)
                    {
                        throw Mapping.Error.InheritanceSubTypeIsAlsoRoot(type.Type);
                    }
                }
                foreach (MetaDataMember member in type.PersistentDataMembers)
                {
                    if (member.IsDeclaredBy(type))
                    {
                        if (member.IsDiscriminator && !this.HasInheritance)
                        {
                            throw Mapping.Error.NonInheritanceClassHasDiscriminator(type);
                        }
                        if (!member.IsAssociation && !string.IsNullOrEmpty(member.MappedName))
                        {
                            string str;
                            object key = InheritanceRules.DistinguishedMemberName(member.Member);
                            if (dictionary.TryGetValue(key, out str))
                            {
                                if (str != member.MappedName)
                                {
                                    throw Mapping.Error.MemberMappedMoreThanOnce(member.Member.Name);
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
                return (this.types != null);
            }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                return this.inheritanceDefault;
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                return this.inheritanceTypes;
            }
        }
    }
}