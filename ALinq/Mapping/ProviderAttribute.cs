using System;

namespace ALinq.Mapping
{
    /// <summary>
    /// Specifies which database provider to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProviderAttribute : Attribute
    {
        // Fields
        private Type providerType;

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.ProviderAttribute class.
        /// </summary>
        public ProviderAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.Mapping.ProviderAttribute class.
        /// </summary>
        /// <param name="type">The provider type to use to construct the ALinq.Mapping.ProviderAttribute.</param>
        public ProviderAttribute(Type type)
        {
            this.providerType = type;
        }

        /// <summary>
        /// Gets the type of the provider that is used to construct the ALinq.Mapping.ProviderAttribute.
        /// </summary>
        /// <returns>
        /// The type of the provider.
        /// </returns>
        public Type Type
        {
            get
            {
                return this.providerType;
            }
        }
    }
}