using System;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using ALinq;
using ALinq.Mapping;
using System.Reflection;
//using OracleConnection = Oracle.DataAccess.Client.OracleConnection;

namespace NorthwindDemo
{
#if !FREE
    //[License("ansiboy", "AV7D47FBDE5376DFA5")]
#endif
    [Provider(typeof(ALinq.Oracle.Odp.OracleProvider))]
    [Database(Name = "Northwind")]
    public class OdpOracleNorthwind : NorthwindDatabase
    {
        private string password;

        public OdpOracleNorthwind(string databaseName, string systemPassword)
            : base(string.Format("Data Source=vpc1;User ID={0};password={1}", databaseName, systemPassword))
        {
            password = systemPassword;
        }

        public static IDbConnection CreateConnection(string databaseName, string systemPassword, string server)
        {
            var str = string.Format("Data Source={0};User ID={1};password={2}",
                                     server, databaseName, systemPassword);
            var conn = new OracleConnection(str);
            return conn;
        }

        public OdpOracleNorthwind(string connection, MappingSource mapping)
            : base(connection, mapping)
        {

        }

        public OdpOracleNorthwind(string connection)
            : base(connection)
        {

        }

        public OdpOracleNorthwind(DbConnection connection)
            : base(connection)
        {

        }

        protected override void ImportData()
        {
            password = "test";
            var instance = new OdpOracleNorthwind("Northwind", password) { Log = Log };

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

        [Function(Name = "NORTHWIND.ADD_CATEGORY")]
        public void AddCategory(
            [Parameter(Name = "CATEGORY_ID")]
            int categoryID,
            [Parameter(Name = "CATEGORY_NAME")]
            string categoryName,
            [Parameter(Name = "CATEGORY_DESCRIPTION")]
            string categoryDescription
            )
        {
            var result = ExecuteMethodCall(this, ((MethodInfo)(MethodBase.GetCurrentMethod())), categoryID, categoryName, categoryDescription).ReturnValue;
            //return (int)result;
        }

        [Function(Name = "NORTHWIND.ADD_CATEGORY1")]
        public void AddCategory1(
            [Parameter(Name = "CATEGORY_ID")]
            int categoryID,
            [Parameter(Name = "CATEGORY_NAME")]
            string categoryName,
            [Parameter(Name = "CATEGORY_DESCRIPTION")]
            string categoryDescription
            )
        {
            var result = ExecuteMethodCall(this, ((MethodInfo)(MethodBase.GetCurrentMethod())), categoryID, categoryName, categoryDescription).ReturnValue;
            //return (decimal)result;
        }

        [Function(Name = "NORTHWIND.ADD_CONTACT")]
        public void AddContact(
            [Parameter(Name = "GUID")]
            Guid guid,
            [Parameter(Name = "ID")]
            out int id
            )
        {
            id = 0;
            ALinq.IExecuteResult result = ExecuteMethodCall(this, ((MethodInfo)(MethodBase.GetCurrentMethod())), guid, id);
            id = ((int)(result.GetParameterValue(1)));
        }

        [Function(Name = "get_customers_count_by_region")]
        public int GetCustomersCountByRegion(
            [Parameter] string region)
        {
            var result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), region);
            return ((int)(result.ReturnValue));
        }

        //[Function(Name = "NORTHWIND.PKG1.GET_CUSTOMERS_BY_CITY")]
        //public ISingleResult<PartialCustomersSetResult> GetCustomersByCity(
        //    [Parameter(DbType = "NVarChar2(20)")] 
        //    string city,
        //    [Parameter(DbType = "Cursor")]
        //    object mycs)
        //{
        //    IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), city, null);
        //    return ((ISingleResult<PartialCustomersSetResult>)(result.ReturnValue));
        //}
        [Function(Name = "NORTHWIND.PKG1.GET_CUSTOMERS_BY_CITY")]
        public virtual ALinq.ISingleResult<PartialCustomersSetResult> GetCustomersByCity([Parameter(Name = "CITY", DbType = "VARCHAR2")] string City, [Parameter(Name = "MYCS", DbType = "REFCURSOR")] out object Mycs)
        {
            Mycs = default(object);
            ALinq.IExecuteResult result = this.ExecuteMethodCall(this, ((System.Reflection.MethodInfo)(System.Reflection.MethodInfo.GetCurrentMethod())), City, Mycs);
            //Mycs = ((object)(result.GetParameterValue(1)));
            return ((ALinq.ISingleResult<PartialCustomersSetResult>)(result.ReturnValue));
        }

        [Function(Name = "PKG2.SINGLE_ROWSET_MULTI_SHAPE")]
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
            [Parameter(DbType = "Cursor")] object mysc1,
            [Parameter(DbType = "Cursor")] object mysc2)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(),
                                                                 customerID, mysc1, mysc2);
            return ((IMultipleResults)(result.ReturnValue));
        }


        [Function(Name = "NORTHWIND.FUNCTION1")]
        public int? Function1()
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod());
            var value = result.GetParameterValue(0);
            return (int?)value;
        }

        [Function(Name = "NORTHWIND.FUNCTION2")]
        public int? Function2(int number)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), number);
            var value = result.GetParameterValue(0);
            return (int?)value;
        }

        [Function(Name = "FUN_STRING1")]
        public string FUN_STRING(int number)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), number);
            var value = result.GetParameterValue(0);
            return (string)value;
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

        [Table]
        public class DUAL
        {

        }

        //public ALinq.Table<DUAL> Dual
        //{
        //    get { return GetTable<DUAL>(); }
        //}

        [Function(Name = "sysdate", IsComposable = true)]
        public new DateTime Now()
        {
            throw new NotImplementedException();
        }
    }
}