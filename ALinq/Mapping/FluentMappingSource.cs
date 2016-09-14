using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ALinq.Mapping
{
    internal sealed class FluentMappingSource : MappingSource
    {
        #region AttributeProvider

        internal class AttributeProvider : IAttributeProvider
        {
            private DataContextMapping mapping;

            public AttributeProvider(DataContextMapping mapping)
            {
                if (mapping == null)
                    throw Error.ArgumentNull("mapping");
                this.mapping = mapping;
            }

            public ProviderAttribute GetProviderAttribute(Type contextType)
            {
                //Debug.Assert(contextType == mapping.DataContextType || contextType.IsSubclassOf(mapping.DataContextType));
                return mapping.Provider;
            }

            public DatabaseAttribute GetDatabaseAttribute(Type contextType)
            {
                //Debug.Assert(contextType == mapping.DataContextType || contextType.IsSubclassOf(mapping.DataContextType));
                return mapping.Database;
            }

            public InheritanceMappingAttribute[] GetInheritanceMappingAttribute(Type type)
            {
                if (type == null)
                    throw Error.ArgumentNull("type");

                EntityMapping entityMapping = this.mapping.GetEntityMapping(type);
                if (entityMapping != null)
                    return entityMapping.InheritanceMappingAttribute.ToArray();

                return null;
            }

            public TableAttribute GetTableAttribute(Type type)
            {
                EntityMapping entityMapping = this.mapping.GetEntityMapping(type);
                if (entityMapping != null)
                    return entityMapping.TableAttribute;

                return null;
            }

            public ColumnAttribute GetColumnAttribute(MemberInfo mi)
            {
                if (mi == null)
                    throw Error.ArgumentNull("mi");

                var entityMapping = this.mapping.GetEntityMapping(mi.DeclaringType);
                if (entityMapping != null)
                    return entityMapping.GetColumn(mi);

                return null;
            }

            public AssociationAttribute GetAssociationAttribute(MemberInfo mi)
            {
                var entityMapping = this.mapping.GetEntityMapping(mi.DeclaringType);
                if (entityMapping != null)
                    return entityMapping.GetAssociation(mi);

                return null;
            }

            public FunctionAttribute GetFunction(MemberInfo mi)
            {
                var entityMapping = this.mapping;//.GetEntityMapping(mi.DeclaringType);
                if (entityMapping != null)
                    return entityMapping.GetFunction(mi);

                return null;
            }

            public ResultTypeAttribute[] GetResultTypeAttributes(MemberInfo mi)
            {
                var entityMapping = this.mapping;//.GetEntityMapping(mi.DeclaringType);
                if (entityMapping != null)
                    return entityMapping.GetResultTypes(mi);

                return null;
            }

            internal DataContextMapping Mapping
            {
                get { return this.mapping; }
            }
        }
        #endregion

        private IDictionary<Type, DataContextMapping> contextMappings;
        private Func<Type, DataContextMapping> getMapping;

        public FluentMappingSource(Func<Type,DataContextMapping> func)
        {
            if (func == null)
                throw Error.ArgumentNull("func");

            contextMappings = new Dictionary<Type, DataContextMapping>();

            this.getMapping = func;
        }

        protected override MetaModel CreateModel(Type dataContextType)
        {
            if (dataContextType == null)
            {
                throw ALinq.Error.ArgumentNull("dataContextType");
            }

            DataContextMapping mapping = getMapping(dataContextType);
            //var type = dataContextType;
            //Debug.Assert(type != null);

            //while (mapping == null && type != typeof(DataContext).BaseType && type != null)
            //{
            //    contextMappings.TryGetValue(type, out mapping);
            //    type = type.BaseType;
            //}


            if (mapping == null)
                throw Error.CouldNotFindMappingForDataContextType(dataContextType);

            var attributeProvider = new AttributeProvider(mapping);
            var model = new AttributedMetaModel(this, dataContextType, attributeProvider);

            return model;
        }

        //public void Add(DataContextMapping dataContextMapping)
        //{
        //    if (dataContextMapping == null)
        //        throw Error.ArgumentNull("dataContextMapping");

        //    if (this.contextMappings.ContainsKey(dataContextMapping.DataContextType))
        //        throw Error.DataContextTypeMappedMoreThanOnce(dataContextMapping.DataContextType);

        //    this.contextMappings[dataContextMapping.DataContextType] = dataContextMapping;
        //}

        //public DataContextMapping GetDataContextMapping(Type dataContextType)
        //{
        //    DataContextMapping value = null;
        //    if (this.contextMappings.TryGetValue(dataContextType, out value))
        //        return value;

        //    return null;
        //}
    }
}