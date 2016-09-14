namespace ALinq
{
    internal sealed class ChangeConflictSession
    {
        // Fields
        private readonly DataContext context;
        private DataContext refreshContext;

        // Methods
        internal ChangeConflictSession(DataContext context)
        {
            this.context = context;
        }

        // Properties
        internal DataContext Context
        {
            get
            {
                return context;
            }
        }

        internal DataContext RefreshContext
        {
            get
            {
                if (refreshContext == null)
                {
                    refreshContext = context.CreateRefreshContext();
                }
                return refreshContext;
            }
        }
    }

 

}
