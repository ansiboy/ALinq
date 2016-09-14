using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;
using System;

namespace ALinq.Mapping
{
    internal class MappedFunction : MetaFunction
    {
        // Fields
        private static readonly ReadOnlyCollection<MetaParameter> _emptyParameters = new List<MetaParameter>(0).AsReadOnly();
        private static readonly ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>(0).AsReadOnly();
        private readonly FunctionMapping map;
        private readonly MethodInfo method;
        private readonly MetaModel model;
        private readonly ReadOnlyCollection<MetaParameter> parameters;
        private readonly MetaParameter returnParameter;
        private readonly ReadOnlyCollection<MetaType> rowTypes;

        // Methods
        internal MappedFunction(MappedMetaModel model, FunctionMapping map, MethodInfo method)
        {
            this.model = model;
            this.map = map;
            this.method = method;
            rowTypes = _emptyTypes;
            if ((map.Types.Count == 0) && (this.method.ReturnType == typeof(IMultipleResults)))
            {
                throw Error.NoResultTypesDeclaredForFunction(method.Name);
            }
            if ((map.Types.Count > 1) && (this.method.ReturnType != typeof(IMultipleResults)))
            {
                throw Error.TooManyResultTypesDeclaredForFunction(method.Name);
            }
            if ((map.Types.Count == 1) && (this.method.ReturnType != typeof(IMultipleResults)))
            {
                Type elementType = TypeSystem.GetElementType(method.ReturnType);
                var list = new List<MetaType>(1) { GetMetaType(map.Types[0], elementType) };
                rowTypes = list.AsReadOnly();
            }
            else if (map.Types.Count > 0)
            {
                var list2 = new List<MetaType>();
                foreach (TypeMapping mapping in map.Types)
                {
                    Type type2 = model.FindType(mapping.Name);
                    if (type2 == null)
                    {
                        throw Error.CouldNotFindElementTypeInModel(mapping.Name);
                    }
                    MetaType metaType = this.GetMetaType(mapping, type2);
                    if (!list2.Contains(metaType))
                    {
                        list2.Add(metaType);
                    }
                }
                this.rowTypes = list2.AsReadOnly();
            }
            else if (map.FunReturn != null)
            {
                this.returnParameter = new MappedReturnParameter(method.ReturnParameter, map.FunReturn);
            }
            ParameterInfo[] parameters = this.method.GetParameters();
            if (parameters.Length > 0)
            {
                var list3 = new List<MetaParameter>(parameters.Length);
                if (this.map.Parameters.Count != parameters.Length)
                {
                    throw Error.IncorrectNumberOfParametersMappedForMethod(this.map.MethodName);
                }
                for (int i = 0; i < parameters.Length; i++)
                {
                    list3.Add(new MappedParameter(parameters[i], this.map.Parameters[i]));
                }
                this.parameters = list3.AsReadOnly();
            }
            else
            {
                this.parameters = _emptyParameters;
            }
        }

        private MetaType GetMetaType(TypeMapping tm, Type elementType)
        {
            MetaTable table = this.model.GetTable(elementType);
            if (table != null)
            {
                return table.RowType.GetInheritanceType(elementType);
            }
            return new MappedRootType((MappedMetaModel)this.model, null, tm, elementType);
        }

        // Properties
        public override bool HasMultipleResults
        {
            get
            {
                return (this.method.ReturnType == typeof(IMultipleResults));
            }
        }

        public override bool IsComposable
        {
            get
            {
                return this.map.IsComposable;
            }
        }

        public override string MappedName
        {
            get
            {
                return this.map.Name;
            }
        }

        public override MethodInfo Method
        {
            get
            {
                return this.method;
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
                return this.method.Name;
            }
        }

        public override ReadOnlyCollection<MetaParameter> Parameters
        {
            get
            {
                return this.parameters;
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
}