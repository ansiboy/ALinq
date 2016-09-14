using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using ALinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public partial class TempTest
    {
        [TestMethod]
        public void Test1()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb") { Log = Console.Out };
            db.DoTransacte(delegate()
            {
                db.Customers.Update(o => new Customer { CompanyName = "XXXX" }, o => o.CustomerID == "kkkkk");
                db.Customers.Delete(o => o.CustomerID == "aaaaa");
            });
          
        }
    }

    public partial class TempTest
    {
        [TestMethod]
        public void Test2()
        {
var str = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Northwind.mdb";
new OleDbConnection(str).DoTransacte(delegate(IDbCommand command)
{
    command.CommandText = @"UPDATE [Customers]
                            SET [CompanyName] = @p0
                            WHERE [CustomerID] = @p1";
    command.Parameters.Add(new OleDbParameter("@p0", "XXXXX"));
    command.Parameters.Add(new OleDbParameter("@p1", "kkkkk"));
    command.ExecuteNonQuery();

    command.CommandText = @"DELETE FROM [Customers] WHERE [CustomerID] = @p0";
    command.Parameters.Add(new OleDbParameter("@p0", "aaaaa"));
    command.ExecuteNonQuery();
});
        }
    }

public static partial class Utility
{
    public static void DoTransacte(this DataContext dc, Action func)
    {
        dc.Connection.Open();
        var tran = dc.Connection.BeginTransaction();
        dc.Transaction = tran;

        try
        {
            func();
            tran.Commit();
        }
        catch
        {
            tran.Rollback();
            throw;
        }
        finally
        {
            dc.Connection.Close();
        }
    }
}

public static partial class Utility
{
    public static void DoTransacte(this IDbConnection connection, Action<IDbCommand> func)
    {
        if (connection == null)
            throw new ArgumentNullException("connection");

        connection.Open();
        var tran = connection.BeginTransaction();
        try
        {
            var command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = tran;
            func(command);
            tran.Commit();
        }
        catch
        {
            tran.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }
}
}
