namespace ALinq.SqlClient
{
    internal abstract class SqlNodeAnnotation
    {
        // Fields
        private string message;

        // Methods
        internal SqlNodeAnnotation(string message)
        {
            this.message = message;
        }

        // Properties
        internal string Message
        {
            get
            {
                return this.message;
            }
        }
    }

 

}