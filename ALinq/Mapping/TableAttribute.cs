using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.Mapping
{
    /// <summary>
    /// Designates a class as an entity class that is associated with a database table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.TableAttribute class.
        /// </summary>
        public TableAttribute()
        {
            
        }

        // Fields
        private string name;

        /// <summary>
        /// Gets or sets the name of the table or view.
        /// </summary>
        /// <returns>
        /// By default, the value is the same as the name of the class.
        /// </returns>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }

 

}
