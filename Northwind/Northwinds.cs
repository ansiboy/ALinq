using System;
using System.Data;
using System.Linq;
using ALinq;
using ALinq.Mapping;
using System.Reflection;
using NorthwindDemo;
using System.Resources;
using ALinq.SqlClient;
using System.Data.SqlClient;


namespace NorthwindDemo
{
    #region Northwinds
    [Database(Name = "Northwind")]
    [Provider(typeof(Sql2005Provider))]
    public class Sql2005Northwind : NorthwindDatabase
    {
        public Sql2005Northwind(SqlConnection connection)
            : base(connection)
        {
        }

        public Sql2005Northwind(string connection)
            : base(connection)
        {
        }

        public class CustomersByCityResult
        {
            public string CustomerID { get; set; }
            public string ContactName { get; set; }
            public string CompanyName { get; set; }
            public string City { get; set; }
        }

        [Function(Name = "[dbo].[CustomersByCity]")]
        public ISingleResult<CustomersByCityResult> CustomersByCity([Parameter(DbType = "NVarChar(20)")] string param1)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), param1);
            return ((ISingleResult<CustomersByCityResult>)(result.ReturnValue));
        }

    }

    [DatabaseAttribute(Name = "Northwind")]
    [Provider(typeof(Sql2000Provider))]
    public class Sql2000Northwind : NorthwindDatabase
    {
        public Sql2000Northwind(SqlConnection connection, XmlMappingSource mapping)
            : base(connection, mapping)
        {
        }
        public Sql2000Northwind(SqlConnection connection)
            : base(connection)
        {

        }
        public class CustomersByCityResult
        {
            public string CustomerID { get; set; }
            public string ContactName { get; set; }
            public string CompanyName { get; set; }
            public string City { get; set; }
        }

        [Function(Name = "[dbo].[CustomersByCity]")]
        public ISingleResult<CustomersByCityResult> CustomersByCity([Parameter(DbType = "NVarChar(20)")] string param1)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), param1);
            return ((ISingleResult<CustomersByCityResult>)(result.ReturnValue));
        }

        [Function]
        public int Function1()
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod());
            var value = result.GetParameterValue(0);
            return (int)value;
        }

        [Function]
        public int Function2(int arg)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), arg);
            var value = result.GetParameterValue(0);
            return (int)value;
        }

    }

    
    #endregion

    public partial class Order
    {
        public int LocalInstanceMethod(int x)
        {
            return x + 1;
        }
    }

    public partial class NorthwindDatabase
    {
        public enum SqlMethodAction
        {
            None,
            InsertContact,
            DeleteConcat,
            UpdateConcat
        }
        public SqlMethodAction SqlAction = SqlMethodAction.None;

        #region SqlMethod

        //partial void InsertSqlMethod(SqlMethod contact)
        //{
        //    SqlAction = SqlMethodAction.InsertContact;
        //}

        //partial void UpdateSqlMethod(SqlMethod original)
        //{
        //    SqlAction = SqlMethodAction.UpdateConcat;
        //}

        //partial void DeleteSqlMethod(SqlMethod contact)
        //{
        //    SqlAction = SqlMethodAction.DeleteConcat;
        //}

        public ResourceManager ResourceManager
        {
            get { return NorthwindDemo.Resource.ResourceManager; }
        }
        #endregion
    }

    [System.Serializable]
    partial class Category
    {

    }

    [System.Serializable]
    partial class Product
    {
        void SetDataContext(DataContext dataContext)
        {
            
        }
    }
}
