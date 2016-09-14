using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ALinq.Mapping;

namespace ALinq.Mapping
{
    internal interface IAttributeProvider
    {
        ProviderAttribute GetProviderAttribute(Type contextType);
        DatabaseAttribute GetDatabaseAttribute(Type contextType);
        InheritanceMappingAttribute[] GetInheritanceMappingAttribute(Type type);
        TableAttribute GetTableAttribute(Type type);
        ColumnAttribute GetColumnAttribute(MemberInfo mi);
        AssociationAttribute GetAssociationAttribute(MemberInfo mi);
        FunctionAttribute GetFunction(MemberInfo mi);
        ResultTypeAttribute[] GetResultTypeAttributes(MemberInfo mi);
    }

    /// <summary>
    /// A mapping source that uses attributes on the context to create the mapping model.
    /// </summary>
    public sealed class AttributeMappingSource : MappingSource
    {
        #region AttributeProvider
        class AttributeProvider : IAttributeProvider
        {
            public ProviderAttribute GetProviderAttribute(Type contextType)
            {
                ProviderAttribute result;
                var customAttributes = (ProviderAttribute[])contextType.GetCustomAttributes(typeof(ProviderAttribute), true);
                if ((customAttributes.Length == 1))
                {
                    result = customAttributes[0];
                }
                else
                {
                    result = new ProviderAttribute();
                }
                return result;
            }

            public DatabaseAttribute GetDatabaseAttribute(Type contextType)
            {
                DatabaseAttribute result = null;
                var customAttributes = (DatabaseAttribute[])contextType.GetCustomAttributes(typeof(DatabaseAttribute), false);
                if ((customAttributes.Length == 1))
                {
                    result = customAttributes[0];
                }

                return result;
            }

            public InheritanceMappingAttribute[] GetInheritanceMappingAttribute(Type type)
            {
                return (InheritanceMappingAttribute[])type.GetCustomAttributes(typeof(InheritanceMappingAttribute), true);
            }

            public TableAttribute GetTableAttribute(Type type)
            {
                var attributes = (TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), false);
                if (attributes.Length > 0)
                    return attributes[0];

                return null;
            }

            public ColumnAttribute GetColumnAttribute(MemberInfo mi)
            {
                return (ColumnAttribute)Attribute.GetCustomAttribute(mi, typeof(ColumnAttribute));
            }

            public AssociationAttribute GetAssociationAttribute(MemberInfo mi)
            {
                return (AssociationAttribute)Attribute.GetCustomAttribute(mi, typeof(AssociationAttribute));
            }

            public FunctionAttribute GetFunction(MemberInfo mi)
            {
                var result = Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute), false) as FunctionAttribute;
                return result;
            }

            public ResultTypeAttribute[] GetResultTypeAttributes(MemberInfo mi)
            {
                var result = (ResultTypeAttribute[])Attribute.GetCustomAttributes(mi, typeof(ResultTypeAttribute));
                return result;
            }
        } 
        #endregion

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.AttributeMappingSource class.
        /// </summary>
        public AttributeMappingSource()
        {
            
        }

        protected override MetaModel CreateModel(Type dataContextType)
        {
            if (dataContextType == null)
            {
                throw ALinq.Error.ArgumentNull("dataContextType");
            }
            var model = new AttributedMetaModel(this, dataContextType, new AttributeProvider());

            return model;
        }
    }
}
