using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents an abstraction of a database table or view.
    /// </summary>
    public abstract class MetaTable
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.MetaTable class.
        /// </summary>
        protected MetaTable()
        {
        }

        /// <summary>
        /// Gets the ALinq.DataContext method that is used to perform delete operations.
        /// </summary>
        /// <returns>
        /// The System.Reflection.MethodInfo that corresponds to the method used for delete operations.
        /// </returns>
        public abstract MethodInfo DeleteMethod { get; }

        /// <summary>
        /// Gets the ALinq.DataContext method that is used to perform insert operations.
        /// </summary>
        /// <returns>
        /// The System.Reflection.MethodInfo that corresponds to the method used for insert operations.
        /// </returns>
        public abstract MethodInfo InsertMethod { get; }

        /// <summary>
        /// Gets the ALinq.Mapping.MetaModel that contains this ALinq.Mapping.MetaTable.
        /// </summary>
        /// <returns>
        /// The ALinq.Mapping.MetaModel that includes this MetaTable.
        /// </returns>
        public abstract MetaModel Model { get; }

        /// <summary>
        /// Gets the ALinq.Mapping.MetaType that describes the type of the rows of the table.
        /// </summary>
        /// <returns>
        /// The type of rows in the table.
        /// </returns>
        public abstract MetaType RowType { get; }

        /// <summary>
        /// Gets the name of the table as defined by the database.
        /// </summary>
        /// <returns>
        /// A string representing the name of the table.
        /// </returns>
        public abstract string TableName { get; }

        /// <summary>
        /// Gets the ALinq.DataContext method that is used to perform update operations.
        /// </summary>
        /// <returns>
        /// The System.Reflection.MethodInfo that corresponds to the method used for update operations.
        /// </returns>
        public abstract MethodInfo UpdateMethod { get; }
    }

 

}
