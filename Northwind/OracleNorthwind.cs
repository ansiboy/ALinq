using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using ALinq;
using ALinq.Mapping;
using System.Data.OracleClient;
using ALinq.Oracle;

namespace NorthwindDemo
{
    //[License("ansiboy", "AV7D47FBDE5376DFA5")]
    [Provider(typeof(OracleProvider))]
    [Database(Name = "Northwind")]
    public class OracleNorthwind : NorthwindDatabase
    {
        #region Constrouctor
        public OracleNorthwind(string conn)
            : base(conn)
        {

        }

        public OracleNorthwind(string databaseName, string systemPassword)
            : base(CreateConnection(databaseName, systemPassword, DB_HOST))
        {

        }

        public OracleNorthwind(string databaseName, string systemPassword, string server)
            : base(CreateConnection(databaseName, systemPassword, server))
        {

        }

        public OracleNorthwind(IDbConnection connection)
            : base(connection)
        {

        }

        public OracleNorthwind(IDbConnection connection, MappingSource mapping)
            : base(connection, mapping)
        {

        }

        public static DbConnection CreateConnection(string databaseName, string systemPassword, string server)
        {
            var builder = new OracleConnectionStringBuilder()
                              {
                                  DataSource = server,
                                  UserID = databaseName,
                                  Password = systemPassword,
                                  PersistSecurityInfo = true,
                              };
            return new OracleConnection(builder.ToString());
        }

        protected override void ImportData()
        {
            var builder = new OracleConnectionStringBuilder()
                              {
                                  UserID = "Northwind",
                                  Password = "Test",
                                  DataSource = DB_HOST,
                              };
            var instance = new OracleNorthwind(new OracleConnection(builder.ToString()), this.Mapping.MappingSource) { Log = Log };

            var data = new NorthwindData();

            instance.Regions.InsertAllOnSubmit(data.regions);
            instance.Employees.InsertAllOnSubmit(data.employees);
            instance.Territories.InsertAllOnSubmit(data.territories);
            instance.EmployeeTerritories.InsertAllOnSubmit(data.employeeTerritories);

            instance.Customers.InsertAllOnSubmit(data.customers);
            instance.Shippers.InsertAllOnSubmit(data.shippers);

            instance.Categories.InsertAllOnSubmit(data.categories);
            instance.Suppliers.InsertAllOnSubmit(data.suppliers);
            instance.Products.InsertAllOnSubmit(data.products);

            instance.Orders.InsertAllOnSubmit(data.orders);
            instance.OrderDetails.InsertAllOnSubmit(data.orderDetails);

            instance.SubmitChanges();

        }
        #endregion

        public override void CreateDatabase()
        {
            base.CreateDatabase();
            var conn = CreateConnection("Northwind", "Test", DB_HOST);
            var command = conn.CreateCommand();
            command.Connection.Open();
            try
            {
                var sb = new StringBuilder();
                #region PROCEDURE AddCategory

                sb.AppendLine("create or replace procedure add_category(category_id in int, category_name in VarChar, ");
                sb.AppendLine("category_description in VarChar,RETURN_VALUE out int) is");
                sb.AppendLine("begin");
                sb.AppendLine("Insert Into Categories");
                sb.AppendLine("(CategoryID,CategoryName, Description)");
                sb.AppendLine("Values (category_id, category_name, category_description);");
                sb.AppendLine("Select category_id into RETURN_VALUE From Dual;");
                sb.AppendLine("end add_category;");
                command.CommandText = @"
create or replace procedure add_category(category_id in int, category_name in VarChar, 
            category_description in VarChar,RETURN_VALUE out int) is
begin

  Insert Into Categories
         (CategoryID,CategoryName, Description)
  Values (category_id, category_name, category_description);
  Select category_id into RETURN_VALUE From Dual;

end add_category;";
                command.CommandText = sb.ToString();
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE GetCustomersCountByRegion
                command.CommandText = @"
create or replace procedure get_customers_count_by_region (
       region in VarChar,  RETURN_VALUE out int) is
begin
    select Count(*) into RETURN_VALUE 
	from Customers 
	where Region = region;
end get_customers_count_by_region;";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE GetCustomersByCity
                command.CommandText = @"
create or replace package pkg1 is
type mytype is ref cursor;
procedure get_customers_by_city(city in VarChar, mycs out mytype, RETURN_VALUE out number);
  
end pkg1 ;";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();

                command.CommandText = @"
create or replace package body pkg1 is

procedure get_customers_by_city(city in VarChar, mycs out mytype, RETURN_VALUE out number) is
begin

open mycs for select CustomerID, ContactName, CompanyName
              from Customers
              where City = city;

RETURN_VALUE := 1;
end get_customers_by_city;

end pkg1;";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE SingleRowsetMultiShape
                command.CommandText = @"
create or replace package pkg2 is
type mytype is ref cursor;
procedure single_rowset_multi_shape(param in number,mycs out mytype,RETURN_VALUE out number);

end pkg2;";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                command.CommandText = @"
create or replace package body pkg2 is
procedure single_rowset_multi_shape(param in number,mycs out mytype,RETURN_VALUE out number) is
begin

if param = 1 then
     open mycs for select *
                   from Customers
                   where Region = 'WA';
end if;
                   
if param = 2 then
    open mycs for  select CustomerID, ContactName, CompanyName
                   from Customers 
                   where City = 'WA'; 
end if;  


                 
RETURN_VALUE := 1;                       
end single_rowset_multi_shape;

end pkg2;";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE GetCustomerAndOrders
                command.CommandText = @"
create or replace package pkg3 is

type mytype1 is ref cursor;
type mytype2 is ref cursor;
procedure get_customer_and_orders(CustomerID in VarChar, mycs1 out mytype1, mycs2 out mytype2, RETURN_VALUE out numeric);

end pkg3;";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                command.CommandText = @"
create or replace package body pkg3 is

procedure get_customer_and_orders(customerID in VarChar, mycs1 out mytype1, mycs2 out mytype2, RETURN_VALUE out numeric) is
begin

open mycs1 for select *
               from Customers
               where CustomerID = customerID;
                   
open mycs2 for  select *
                from Orders 
                where CustomerID = customerID; 
  
           
RETURN_VALUE := 1;                       
end get_customer_and_orders;

end pkg3;";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion
            }
            finally
            {
                command.Connection.Close();
            }
        }

        [Function(Name = "northwind.add_category")]
        public new int AddCategory(
            [Parameter(Name = "category_id")]
            int categoryID,
            [Parameter(Name = "category_name")]
            string categoryName,
            [Parameter(Name = "category_description")]
            string categoryDescription
            )
        {
            var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(),
                                           categoryID, categoryName, categoryDescription).ReturnValue;
            return (int)result;
        }

        [Function(Name = "northwind.get_customers_count_by_region")]
        public int GetCustomersCountByRegion(
            [Parameter] 
            string region)
        {
            var result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), region);
            return ((int)(result.ReturnValue));
        }

        [Function(Name = "pkg_customers_by_city.get")]
        public ISingleResult<PartialCustomersSetResult> GetCustomersByCity(
            [Parameter(DbType = "NVarChar(20)")] 
            string city,
            [Parameter(DbType = "Cursor")]
            object mycs)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), city, null);
            return ((ISingleResult<PartialCustomersSetResult>)(result.ReturnValue));
        }

        [Function(Name = "northwind.pkg_Single_Rowset_Multi_Shape.get")]
        [ResultType(typeof(Customer))]
        [ResultType(typeof(PartialCustomersSetResult))]
        public IMultipleResults SingleRowset_MultiShape(
            [Parameter] int? param,
            [Parameter(DbType = "Cursor")]  object mycs)
        {
            IExecuteResult result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), param, mycs);
            return ((IMultipleResults)(result.ReturnValue));
        }

        [Function(Name = "northwind.pkg_customer_and_orders.get")]
        [ResultType(typeof(Customer))]
        [ResultType(typeof(Order))]
        public IMultipleResults GetCustomerAndOrders(
            [Parameter] string customerID,
            [Parameter(DbType = "Cursor")] object mycs1,
            [Parameter(DbType = "Cursor")] object mycs2)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(),
                                                                 customerID, mycs1, mycs2);
            return ((IMultipleResults)(result.ReturnValue));
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

        [Function(Name = "sysdate", IsComposable = true)]
        public override DateTime Now()
        {
            throw new NotImplementedException();
        }


    }
}