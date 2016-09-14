using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using ALinq;

namespace ALinq.SqlClient
{
    internal class SqlConnectionManager : ISqlConnectionManager
    {
        // Fields
        private readonly DbConnection connection;
        private readonly SqlInfoMessageEventHandler infoMessagehandler;
        private readonly int maxUsers;
        private readonly IProvider provider;
        private Transaction systemTransaction;
        private DbTransaction transaction;
        private readonly List<IConnectionUser> users;

        // Methods
        public SqlConnectionManager(IProvider provider, DbConnection con, int maxUsers)
        {
            this.provider = provider;
            connection = con;
            this.maxUsers = maxUsers;
            infoMessagehandler = OnInfoMessage;
            users = new List<IConnectionUser>(maxUsers);
        }

        private void AddInfoMessageHandler()
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                sqlConnection.InfoMessage += infoMessagehandler;
            }
        }

        private void BootUser(IConnectionUser user)
        {
            bool close = AutoClose;
            AutoClose = false;
            int index = users.IndexOf(user);
            if (index >= 0)
            {
                users.RemoveAt(index);
            }
            user.CompleteUse();
            AutoClose = close;
        }

        public void ClearConnection()
        {
            while (users.Count > 0)
            {
                BootUser(users[0]);
            }
        }

        private void CloseConnection()
        {
            if ((connection != null) && (connection.State != ConnectionState.Closed))
            {
                connection.Close();
            }
            RemoveInfoMessageHandler();
            AutoClose = false;
        }

        public void DisposeConnection()
        {
            if (AutoClose)
            {
                CloseConnection();
            }
        }

        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            if (provider.Log != null)
            {
                provider.Log.WriteLine(Strings.LogGeneralInfoMessage(args.Source, args.Message));
            }
        }

        private void OnTransactionCompleted(object sender, TransactionEventArgs args)
        {
            if ((users.Count == 0) && AutoClose)
            {
                CloseConnection();
            }
        }

        public void ReleaseConnection(IConnectionUser user)
        {
            if (user == null)
            {
                throw Error.ArgumentNull("user");
            }
            int index = users.IndexOf(user);
            if (index >= 0)
            {
                users.RemoveAt(index);
            }
            if (((users.Count == 0) && AutoClose) && ((transaction == null) &&
                                                                (System.Transactions.Transaction.Current == null)))
            {
                CloseConnection();
            }
        }

        private void RemoveInfoMessageHandler()
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                sqlConnection.InfoMessage -= infoMessagehandler;
            }
        }

        public DbConnection UseConnection(IConnectionUser user)
        {
            if (user == null)
            {
                throw Error.ArgumentNull("user");
            }
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                AutoClose = true;
                AddInfoMessageHandler();
                if (System.Transactions.Transaction.Current != null)
                {
                    System.Transactions.Transaction.Current.TransactionCompleted += OnTransactionCompleted;
                }
            }
            if (((transaction == null) && (System.Transactions.Transaction.Current != null)) &&
                (System.Transactions.Transaction.Current != systemTransaction))
            {
                ClearConnection();
                systemTransaction = System.Transactions.Transaction.Current;
                connection.EnlistTransaction(System.Transactions.Transaction.Current);
            }
            if (users.Count == maxUsers)
            {
                BootUser(users[0]);
            }
            users.Add(user);
            return connection;
        }

        // Properties
        public bool AutoClose { get; set; }

        public DbConnection Connection
        {
            get { return connection; }
        }

        public int MaxUsers
        {
            get { return maxUsers; }
        }

        public DbTransaction Transaction
        {
            get { return transaction; }
            set
            {
                if (value != transaction)
                {
                    if ((value != null) && !(connection.Equals(value.Connection)))
                    {
                        throw Error.TransactionDoesNotMatchConnection();
                    }
                    transaction = value;
                }
            }
        }
    }

    
}