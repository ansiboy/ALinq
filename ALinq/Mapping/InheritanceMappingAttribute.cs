using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Maps an inheritance hierarchy in a LINQ to SQL application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class InheritanceMappingAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.InheritanceMappingAttribute class.
        /// </summary>
        public InheritanceMappingAttribute()
        {
            
        }

        /// <summary>
        /// Gets or sets the discriminator code value in a mapped inheritance hierarchy.
        /// </summary>
        /// <returns>
        /// Must be user-specified. There is no default value.
        /// </returns>
        public object Code { get; set; }

        /// <summary>
        /// Gets or sets whether an object of this type in instantiated when the discriminator value does not match a specified value.
        /// </summary>
        /// <returns>
        /// Default = false.
        /// </returns>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the type of the class in the hierarchy.
        /// </summary>
        /// <returns>
        /// Must be user-specified. There is no default value.
        /// </returns>
        public Type Type { get; set; }
    }
}