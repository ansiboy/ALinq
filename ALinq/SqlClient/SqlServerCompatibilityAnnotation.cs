using System;

namespace ALinq.SqlClient
{
    internal class SqlServerCompatibilityAnnotation : SqlNodeAnnotation
    {
        // Fields
        private SqlProvider.ProviderMode[] providers;

        // Methods
        internal SqlServerCompatibilityAnnotation(string message, params SqlProvider.ProviderMode[] providers)
            : base(message)
        {
            this.providers = providers;
        }

        internal bool AppliesTo(SqlProvider.ProviderMode provider)
        {
            foreach (SqlProvider.ProviderMode mode in this.providers)
            {
                if (mode == provider)
                {
                    return true;
                }
            }
            return false;
        }
    }


}