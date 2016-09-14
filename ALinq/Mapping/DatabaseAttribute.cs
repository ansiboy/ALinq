using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.Mapping
{
    /// <summary>
    /// Specifies certain attributes of a class that represents a database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DatabaseAttribute : Attribute
    {
        // Fields
        private string name;

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.DatabaseAttribute class.
        /// </summary>
        public DatabaseAttribute()
        {
            
        }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <returns>
        /// The name.
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
