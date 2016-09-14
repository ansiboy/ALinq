using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal sealed class AttributedMetaFunction : MetaFunction
    {
        // Fields
        private static readonly ReadOnlyCollection<MetaParameter> _emptyParameters = new List<MetaParameter>(0).AsReadOnly();
        private static readonly ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>(0).AsReadOnly();
        private readonly FunctionAttribute functionAttrib;
        private readonly MethodInfo methodInfo;
        private readonly AttributedMetaModel model;
        private readonly ReadOnlyCollection<MetaParameter> parameters;
        private readonly MetaParameter returnParameter;
        private readonly ReadOnlyCollection<MetaType> rowTypes;
        private IAttributeProvider attributeProvider;

        // Methods
        public AttributedMetaFunction(AttributedMetaModel model, MethodInfo mi, IAttributeProvider attributeProvider)
        {
            if (model == null)
                throw Error.ArgumentNull("model");

            if (mi == null)
                throw Error.ArgumentNull("mi");

            if (attributeProvider == null)
                throw Error.ArgumentNull("attributeProvider");

            this.attributeProvider = attributeProvider;

            this.model = model;
            this.methodInfo = mi;
            this.rowTypes = _emptyTypes;
            //this.functionAttrib = Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute), false) as FunctionAttribute;
            this.functionAttrib = this.attributeProvider.GetFunction(mi);


            //(ResultTypeAttribute[])Attribute.GetCustomAttributes(mi, typeof(ResultTypeAttribute));
            var resultTypeAttributes = attributeProvider.GetResultTypeAttributes(mi);
            if (resultTypeAttributes != null && resultTypeAttributes.Length == 0 && mi.ReturnType == typeof(IMultipleResults))
            {
                throw Mapping.Error.NoResultTypesDeclaredForFunction(mi.Name);
            }
            if (resultTypeAttributes != null && resultTypeAttributes.Length > 1 && mi.ReturnType != typeof(IMultipleResults))
            {
                throw Mapping.Error.TooManyResultTypesDeclaredForFunction(mi.Name);
            }
            if (((resultTypeAttributes != null && resultTypeAttributes.Length <= 1) && mi.ReturnType.IsGenericType) &&
                (((mi.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                (mi.ReturnType.GetGenericTypeDefinition() == typeof(ISingleResult<>))) ||
                (mi.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))))
            {
                Type elementType = TypeSystem.GetElementType(mi.ReturnType);
                var list = new List<MetaType>(1);
                list.Add(this.GetMetaType(elementType));
                this.rowTypes = list.AsReadOnly();
            }
            else if (resultTypeAttributes != null && resultTypeAttributes.Length > 0)
            {
                var list2 = new List<MetaType>();
                foreach (ResultTypeAttribute attribute in resultTypeAttributes)
                {
                    Type type = attribute.Type;
                    MetaType metaType = this.GetMetaType(type);
                    if (!list2.Contains(metaType))
                    {
                        list2.Add(metaType);
                    }
                }
                this.rowTypes = list2.AsReadOnly();
            }
            else
            {
                this.returnParameter = new AttributedMetaParameter(this.methodInfo.ReturnParameter);
            }
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length > 0)
            {
                var list3 = new List<MetaParameter>(parameters.Length);
                int index = 0;
                int length = parameters.Length;
                while (index < length)
                {
                    var item = new AttributedMetaParameter(parameters[index]);
                    list3.Add(item);
                    index++;
                }
                this.parameters = list3.AsReadOnly();
            }
            else
            {
                this.parameters = _emptyParameters;
            }
        }

        private MetaType GetMetaType(Type type)
        {
            MetaTable tableNoLocks = this.model.GetTableNoLocks(type);
            if (tableNoLocks != null)
            {
                return tableNoLocks.RowType.GetInheritanceType(type);
            }
            return new AttributedRootType(this.model, null, type);
        }

        // Properties
        public override bool HasMultipleResults
        {
            get
            {
                return (this.methodInfo.ReturnType == typeof(IMultipleResults));
            }
        }

        public override bool IsComposable
        {
            get
            {
                return this.functionAttrib.IsComposable;
            }
        }

        public override string MappedName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.functionAttrib.Name))
                {
                    return this.functionAttrib.Name;
                }
                return this.methodInfo.Name;
            }
        }

        public override MethodInfo Method
        {
            get
            {
                return this.methodInfo;
            }
        }

        public override MetaModel Model
        {
            get
            {
                return this.model;
            }
        }

        public override string Name
        {
            get
            {
                return this.methodInfo.Name;
            }
        }

        public override ReadOnlyCollection<MetaParameter> Parameters
        {
            get
            {
                return parameters;
            }
        }

        public override ReadOnlyCollection<MetaType> ResultRowTypes
        {
            get
            {
                return this.rowTypes;
            }
        }

        public override MetaParameter ReturnParameter
        {
            get
            {
                return this.returnParameter;
            }
        }
    }


    internal class EntityRefDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class
    {
        // Fields
        private MetaAccessor<T, EntityRef<V>> acc;

        // Methods
        internal EntityRefDefSourceAccessor(MetaAccessor<T, EntityRef<V>> acc)
        {
            this.acc = acc;
        }

        public override IEnumerable<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance).Source;
        }

        public override void SetValue(ref T instance, IEnumerable<V> value)
        {
            EntityRef<V> ref2 = this.acc.GetValue(instance);
            if (ref2.HasAssignedValue || ref2.HasLoadedValue)
            {
                throw Error.EntityRefAlreadyLoaded();
            }
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
    }




    internal class EntityRefDefValueAccessor<T, V> : MetaAccessor<T, V> where V : class
    {
        // Fields
        private MetaAccessor<T, EntityRef<V>> acc;

        // Methods
        internal EntityRefDefValueAccessor(MetaAccessor<T, EntityRef<V>> acc)
        {
            this.acc = acc;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).UnderlyingValue;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
    }




    internal class EntityRefValueAccessor<T, V> : MetaAccessor<T, V> where V : class
    {
        // Fields
        private MetaAccessor<T, EntityRef<V>> acc;

        // Methods
        internal EntityRefValueAccessor(MetaAccessor<T, EntityRef<V>> acc)
        {
            this.acc = acc;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).Entity;
        }

        public override bool HasAssignedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasAssignedValue;
        }

        public override bool HasLoadedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasLoadedValue;
        }

        public override bool HasValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasValue;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
    }




    internal class LinkDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>>
    {
        // Fields
        private MetaAccessor<T, Link<V>> acc;

        // Methods
        internal LinkDefSourceAccessor(MetaAccessor<T, Link<V>> acc)
        {
            this.acc = acc;
        }

        public override IEnumerable<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance).Source;
        }

        public override void SetValue(ref T instance, IEnumerable<V> value)
        {
            Link<V> link = this.acc.GetValue(instance);
            if (link.HasAssignedValue || link.HasLoadedValue)
            {
                throw Mapping.Error.LinkAlreadyLoaded();
            }
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }




    internal class LinkDefValueAccessor<T, V> : MetaAccessor<T, V>
    {
        // Fields
        private MetaAccessor<T, Link<V>> acc;

        // Methods
        internal LinkDefValueAccessor(MetaAccessor<T, Link<V>> acc)
        {
            this.acc = acc;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).UnderlyingValue;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }




    internal class LinkValueAccessor<T, V> : MetaAccessor<T, V>
    {
        // Fields
        private MetaAccessor<T, Link<V>> acc;

        // Methods
        internal LinkValueAccessor(MetaAccessor<T, Link<V>> acc)
        {
            this.acc = acc;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).Value;
        }

        public override bool HasAssignedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasAssignedValue;
        }

        public override bool HasLoadedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasLoadedValue;
        }

        public override bool HasValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasValue;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }

    /// <summary>
    /// Enables specification of mapping details for a stored procedure method parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ParameterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.ParameterAttribute class.
        /// </summary>
        public ParameterAttribute()
        {

        }

        /// <summary>
        /// Gets or sets the type of the parameter for a provider-specific database.
        /// </summary>
        /// <returns>
        /// The type as a string.
        /// </returns>
        public string DbType { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <returns>
        /// The name as a string.
        /// </returns>
        public string Name { get; set; }
    }
}