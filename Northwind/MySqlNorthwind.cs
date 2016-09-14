using System;
using System.Data.Common;
using System.Reflection;
using ALinq;
using ALinq.Mapping;
using ALinq.MySQL;
using MySql.Data.MySqlClient;

namespace NorthwindDemo
{
    public class PartialCustomersSetResult
    {
        [Column]
        public string CustomerID;

        [Column]
        public string ContactName;

        [Column]
        public string CompanyName;
    }

    //[License("ansiboy", "572E7B3C3B86ED629B9178A19CFAADEC")]
    [Provider(typeof(MySqlProvider))]
    public class MySqlNorthwind : NorthwindDatabase
    {
        #region Consturctor
        public MySqlNorthwind(string connection, MappingSource mapping)
            : base(connection, mapping)
        {

        }

        public MySqlNorthwind(string connection)
            : base(connection)
        {

        }

        public static DbConnection CreateConnection(string userID, string password,
                                                    string database, string server, uint port)
        {
            var builder = new MySqlConnectionStringBuilder
                              {
                                  Server = server,
                                  Port = port,
                                  UserID = userID,
                                  Password = password,
                                  Database = database,
                                  AllowZeroDateTime = true,
                              };
            return new MySqlConnection(builder.ToString());
        }

        #endregion

        #region MyRegion
        //        public override void CreateDatabase()
        //        {
        //            base.CreateDatabase();
        //            var conn = Connection;
        //            conn.Open();
        //            var command = conn.CreateCommand();
        //            command.CommandText = @"Create PROCEDURE GetCustomersCountByRegion (pRegion CHAR(15), OUT RETURN_VALUE int)
        //                                    BEGIN 
        //                                            Select Count(*) INTO RETURN_VALUE From Customers
        //                                            Where Region = pRegion;
        //                                    END";
        //            if (Log != null)
        //            {
        //                Log.WriteLine(command.CommandText);
        //                Log.WriteLine();
        //            }
        //            command.ExecuteNonQuery();

        //            command.CommandText = @"Create PROCEDURE AddCategory (pName CHAR(15), pDescription Text)
        //                                    BEGIN 
        //                                            Insert Into Categories ( CategoryName, Description )
        //                                            Values ( pName, pDescription );
        //                                    END";
        //            if (Log != null)
        //            {
        //                Log.WriteLine(command.CommandText);
        //                Log.WriteLine();
        //            }
        //            command.ExecuteNonQuery();

        //            command.CommandText = @"Create PROCEDURE GetCustomersByCity (pCity Char(30))
        //                                    BEGIN 
        //                                            Select CustomerID, ContactName, CompanyName 
        //                                            From Customers
        //                                            Where Customers.City = pCity;
        //                                    END";
        //            if (Log != null)
        //            {
        //                Log.WriteLine(command.CommandText);
        //                Log.WriteLine();
        //            }
        //            command.ExecuteNonQuery();

        //            command.CommandText = @"Create PROCEDURE SingleRowset_MultiShape (param INT)
        //                                    BEGIN 
        //                                            If param = 1 Then
        //                                                Select CustomerID, ContactName, CompanyName 
        //                                                From Customers;
        //                                            End If;
        //                                            
        //                                            If param = 2 Then
        //                                                Select * From Customers;
        //                                            End If;
        //                                    END";
        //            if (Log != null)
        //            {
        //                Log.WriteLine(command.CommandText);
        //                Log.WriteLine();
        //            }
        //            command.ExecuteNonQuery();

        //            command.CommandText = @"Create PROCEDURE GetCustomerAndOrders (pCustomerID Char(15))
        //                                    BEGIN 
        //                                            Select * From Customers Where CustomerID = pCustomerID;
        //                                            Select * From Orders Where CustomerID = pCustomerID;
        //                                    END";
        //            if (Log != null)
        //            {
        //                Log.WriteLine(command.CommandText);
        //                Log.WriteLine();
        //            }
        //            command.ExecuteNonQuery();
        //        }

        //[Function(Name = "`northwind`.`GetEmployee`")]
        //public ALinq.ISingleResult<GetEmployeeByIDResult> GetEmployee(
        //    [Parameter]
        //        int employeeID)
        //{
        //    IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), employeeID);
        //    return ((ISingleResult<GetEmployeeByIDResult>)(result.ReturnValue));
        //}

        //1????? 
        #endregion
        [Function(Name = "northwind.GetCustomersCountByRegion")]
        public int GetCustomersCountByRegion(
            [Parameter] 
            string region)
        {
            var result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), region);
            return ((int)(result.ReturnValue));
        }

        //2????????
        [Function(Name = "northwind.GetCustomersByCity")]
        public ISingleResult<PartialCustomersSetResult> GetCustomersByCity(
            [Parameter(DbType = "NVarChar(20)")] string param)
        {
            IExecuteResult result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), param);
            return ((ISingleResult<PartialCustomersSetResult>)(result.ReturnValue));
        }



        //3.????????????
        [Function(Name = "northwind.SingleRowset_MultiShape")]
        [ResultType(typeof(Customer))]
        [ResultType(typeof(PartialCustomersSetResult))]
        public IMultipleResults SingleRowset_MultiShape(
            [Parameter] int? param)
        {
            IExecuteResult result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), param);
            return ((IMultipleResults)(result.ReturnValue));
        }

        [Function(Name = "northwind.GetCustomerAndOrders")]
        [ResultType(typeof(Customer))]
        [ResultType(typeof(Order))]
        public IMultipleResults GetCustomerAndOrders(
            [Parameter] string customerID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), customerID);
            return ((IMultipleResults)(result.ReturnValue));
        }

        [Function(IsComposable = true)]
        public string Datediff(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }



    }
}