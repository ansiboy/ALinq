using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq
{
    /// <summary>
    /// Provides access to the return value or results of executing a query.
    /// </summary>
    public interface IExecuteResult : IDisposable
    {
        /// <summary>
        /// Provides access to the nth output parameter.
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to be retrieved.</param>
        /// <returns>An object that contains the value of the specified parameter.</returns>
        object GetParameterValue(int parameterIndex);

        /// <summary>
        /// Gets the return value or result of the executed query.
        /// </summary>
        /// <returns>
        /// The value or result of the executed query.
        /// </returns>
        object ReturnValue { get; }
    }





}
