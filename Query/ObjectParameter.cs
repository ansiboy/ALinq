using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.Dynamic
{
    public class ObjectParameter
    {
      

        private object value;

        internal ObjectParameter(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            this.Value = value;

        }


        /// <summary>
        /// Initializes a new instance of the ALinq.Dynamic.ObjectParameter class with the specified name and value.
        /// </summary>
        /// <param name="name">The parameter name. This name should not include the "@" parameter marker that is used in Entity SQL statements, only the actual name. The first character of the expression must be a letter. Any successive characters in the expression must be either letters, numbers, or an underscore (_) character.</param>
        /// <param name="value">The initial value (and inherently, the type) of the parameter.</param>
        public ObjectParameter(string name, object value)
            : this(value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.Name = name;
        }

        /// <summary>
        /// Gets the parameter name, which can only be set through a constructor.
        /// </summary>
        /// <returns>
        /// The parameter name, which can only be set through a constructor.
        /// </returns>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the parameter value.
        /// </summary>
        /// <returns>The parameter value.</returns>
        public object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (value != null)
                {
                    this.ParameterType = value.GetType();
                }
            }
        }

        public Type ParameterType { get; private set; }
    }
}
