using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq
{
    /// <summary>
    /// Defines how the Overload:ALinq.DataContext.Refresh method handles optimistic concurrency conflicts.
    /// </summary>
    public enum RefreshMode
    {
        /// <summary>
        /// Forces the Overload:ALinq.DataContext.Refresh method to swap the original value with the values retrieved from the database. No current value is modified.
        /// </summary>
        KeepCurrentValues,
        /// <summary>
        /// Forces the Overload:ALinq.DataContext.Refresh method to keep the current value that has been changed, but updates the other values with the database values.
        /// </summary>
        KeepChanges,
        /// <summary>
        /// Forces the Overload:ALinq.DataContext.Refresh method to override all the current values with the values from the database.
        /// </summary>
        OverwriteCurrentValues
    }

 

 

}
