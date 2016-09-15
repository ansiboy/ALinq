using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ALinq.Dynamic
{
    class ObjectParameterCollection : IEnumerable<ObjectParameter>
    {
        private Dictionary<string, ObjectParameter> items;
        private int num = -1;

        public ObjectParameterCollection()
        {
            this.items = new Dictionary<string, ObjectParameter>();
        }

        public void Add(object value)
        {
            ObjectParameter p;
            if (value is ObjectParameter)
            {
                p = (ObjectParameter)value;
            }
            else
            {
                var name = UniqueName();
                p = new ObjectParameter(name, value);
            }

            Add(p);
        }

        public void Add(ObjectParameter parameter)
        {
            if (parameter == null)
                throw Error.ArgumentNull("parameter");

            if (items.ContainsKey(parameter.Name))
                throw Error.ObjectParameterCollection_DuplicateParameterName(parameter.Name);

            ValidteParameterType(parameter);

            items.Add(parameter.Name, parameter);
        }

        private static Type[] ScalarTypes = new[]
        {
            typeof(bool), typeof(char), typeof(string), typeof(sbyte), 
			typeof(byte), typeof(short), typeof(ushort), typeof(int), 
			typeof(uint), typeof(long), typeof(ulong), typeof(float), 
			typeof(double), typeof(decimal), typeof(Guid), typeof(DateTime)
        };

        static void ValidteParameterType(ObjectParameter parameter)
        {
            if (parameter.Value != null)
            {
                var type = parameter.ParameterType;
                Debug.Assert(type != null);

                if (ScalarTypes.Contains(type))
                    return;

                if (typeof(Enum).IsAssignableFrom(type))
                    return;

                if (typeof(IEnumerable<string>).IsAssignableFrom(type))
                    return;

                if (typeof(IEnumerable<Guid>).IsAssignableFrom(type))
                    return;

                if (typeof(Expression).IsAssignableFrom(type))
                    return;

                if (typeof(IQueryable).IsAssignableFrom(type))
                    return;

                throw Error.InvalidParameterType(type.FullName);
            }

        }

        string UniqueName()
        {
            num = num + 1;
            while (items.ContainsKey(num.ToString()))
            {
                num = num + 1;
            }
            return num.ToString();
        }

        #region Implementation of IEnumerable

        public IEnumerator<ObjectParameter> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
