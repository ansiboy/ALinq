using System;
using System.Data;
using System.Data.Common;
using ALinq.Mapping;
using System.Data.OleDb;

namespace NorthwindDemo
{
    //[ALinq.License("ansiboy", "74370761A054A821B1F21F9D4CAE1168")]
    [Provider(typeof(ALinq.Access.AccessDbProvider))]
    public class AccessNorthwind : NorthwindDatabase
    {
        public AccessNorthwind(string fileName)
            : base(fileName)
        {
        }

        public AccessNorthwind(OleDbConnection connection)
            : base(connection)
        {

        }

        public AccessNorthwind(string conn, MappingSource mapping)
            : base(conn, mapping)
        {

        }

        public static DbConnection CreateConnection(string fileName)
        {
            var builder = new OleDbConnectionStringBuilder
                              {
                                  DataSource = fileName,
                                  Provider = "Microsoft.Jet.OLEDB.4.0"
                              };
            return new OleDbConnection(builder.ConnectionString);
        }

        public override void CreateDatabase()
        {
            base.CreateDatabase();
            var conn = Connection;
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = @"Create PROCEDURE GetUserByID 
                                    (
                                        [userID] Int
                                    )
                                    AS
                                    SELECT *
                                    FROM [User]
                                    Where ID = userID;";
            if (Log != null)
            {
                Log.WriteLine(command.CommandText);
                Log.WriteLine();
            }
            ExecuteCommand(command.CommandText);

            command.CommandText = @"Create PROCEDURE AddUser
                                    (
                                        [mag] Char(30),
                                        [ast] Char(30),
                                        [dep] Char(30)
                                    ) 
                                    AS
                                    Insert Into [User]
                                          (Manager,Assistant,Department)
                                    Values([mag],[ast],[dep])";
            if (Log != null)
            {
                Log.WriteLine(command.CommandText);
                Log.WriteLine();
            }
            command.ExecuteNonQuery();

            command.CommandText = @"Create PROCEDURE AddCategory
                                    AS
                                    Insert Into [Categories]
                                           (CategoryName, Description)
                                    Values ([@name], [@description])";
            if (Log != null)
            {
                Log.WriteLine(command.CommandText);
                Log.WriteLine();
            }
            command.ExecuteNonQuery();

            command.CommandText = @"Create PROCEDURE GetCustomersCountByRegion
                                    AS
                                    Select Count(*) From Customers
                                    Where Region = [@region]";
            if (Log != null)
            {
                Log.WriteLine(command.CommandText);
                Log.WriteLine();
            }
            command.ExecuteNonQuery();

            command.CommandText = @"Create PROCEDURE GetCustomersByCity
                                    AS
                                    Select CustomerID, ContactName, CompanyName
                                    From Customers
                                    Where City = [@city]";
            if (Log != null)
            {
                Log.WriteLine(command.CommandText);
                Log.WriteLine();
            }
            command.ExecuteNonQuery();


            //ExecuteCommand(command.CommandText);
            conn.Close();
        }

      


        public class PartialCustomersSetResult
        {
            [Column]
            public string CustomerID;

            [Column]
            public string ContactName;

            [Column]
            public string CompanyName;
        }


    }
}