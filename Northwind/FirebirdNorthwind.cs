using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
#if !FREE
    //[License("ansiboy", "EC4E10904943EE7F5C9C830F80CC1693")]
#endif
    [Provider(typeof(ALinq.Firebird.FirebirdProvider))]
    public class FirebirdNorthwind : NorthwindDatabase
    {
        public FirebirdNorthwind(string connection, XmlMappingSource xmlMapping)
            : base(connection, xmlMapping)
        {
        }

        public FirebirdNorthwind(string connection)
            : base(connection)
        {
        }

        public FirebirdNorthwind(IDbConnection connection)
            :base(connection)
        {
            
        }


        public override void CreateDatabase()
        {
            base.CreateDatabase();
            return;
            var conn = Connection;
            var command = conn.CreateCommand();
            conn.Open();
            try
            {
                #region PROCEDURE AddCategory
                command.CommandText = @"


CREATE PROCEDURE AddCategory (
 P1           VARCHAR(20),
 P2           VARCHAR(20))
RETURNS (
 RETURN_VALUE INTEGER)
AS 
BEGIN
  Insert Into Categories (CategoryID, CategoryName, Description)
  Values (NEXT VALUE FOR Seq_Categories, :p1, :p2); 
  
  SELECT GEN_ID(Seq_Categories, 0) FROM RDB$DATABASE into RETURN_VALUE;
  SUSPEND ;
END; ";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE GetCustomersByCity
                command.CommandText = @"
--Procedure: GetCustomersByCity

--DROP PROCEDURE GetCustomersByCity;

RECREATE PROCEDURE GetCustomersByCity (
 CITY         VARCHAR(20))
RETURNS (
 P0           VARCHAR(50),
 P2           VARCHAR(50),
 P1           VARCHAR(50),
 RETURN_VALUE VARCHAR(50))
AS 
BEGIN
    for select CustomerID, ContactName, CompanyName
	from Customers 
	where City = :city
    into :p0, :p1, :p2 do
    suspend;
END; ";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE GetCustomersCountByRegion
                command.CommandText = @"
--Procedure: GetCustomersCountByRegion

--DROP PROCEDURE GetCustomersCountByRegion;

CREATE PROCEDURE GetCustomersCountByRegion (
 P1           VARCHAR(20))
RETURNS (
 RETURN_VALUE INTEGER)
AS 
BEGIN
     select count(*) from Customers 
     where Region = :P1 
     into :RETURN_VALUE;
END";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE SingleRowset_MultiShape
                command.CommandText = @"
RECREATE PROCEDURE SINGLEROWSET_MULTISHAPE (
 P0          INTEGER)
RETURNS (
 CUSTOMERID  VARCHAR(50),
 CONTACTNAME VARCHAR(50),
 COMPANYNAME VARCHAR(50),
 ADDRESS     VARCHAR(50),
 CITY        VARCHAR(50),
 REGION      VARCHAR(50),
 POSTALCODE  VARCHAR(50),
 COUNTRY     VARCHAR(50),
 PHONE       VARCHAR(50),
 FAX         VARCHAR(20))
AS 
BEGIN

if(:p0 = 1) then 
    for select CustomerID, ContactName, CompanyName
    from Customers
    where Region = 'WA'
    into :customerID, :ContactName, :CompanyName do
    suspend;
    
if(:p0 = 2) then
    for select CustomerID, ContactName, CompanyName, Address,
               City, Region, PostalCode, Country, Phone,Fax
	from Customers 
	where Region = 'WA'
    into :customerID, :ContactName, :CompanyName, :Address, :City, :Region, 
         :PostalCode, :Country, :Phone, :Fax do
    suspend;
    
  
END";
                if (Log != null)
                {
                    Log.WriteLine(command.CommandText);
                    Log.WriteLine();
                }
                command.ExecuteNonQuery();
                #endregion

                #region PROCEDURE GetCustomerAndOrders
                command.CommandText = @"
--Procedure: GETCUSTOMERANDORDERS

--DROP PROCEDURE GETCUSTOMERANDORDERS;

CREATE PROCEDURE GETCUSTOMERANDORDERS (
 PARAM          VARCHAR(15))
RETURNS (
 CUSTOMERID     VARCHAR(20),
 COMPANYNAME    VARCHAR(20),
 CONTACTNAME    VARCHAR(20),
 CONTACTTITLE   VARCHAR(30),
 ADDRESS        VARCHAR(20),
 CITY           VARCHAR(20),
 REGION         VARCHAR(20),
 POSTALCODE     VARCHAR(20),
 COUNTRY        VARCHAR(20),
 PHONE          VARCHAR(20),
 FAX            VARCHAR(20),
 ORDERID        VARCHAR(20),
 EMPLOYEEID     INTEGER,
 ORDERDATE      TIMESTAMP,
 REQUIREDDATE   TIMESTAMP,
 SHIPPEDDATE    TIMESTAMP,
 SHIPVIA        INTEGER,
 FREIGHT        FLOAT,
 SHIPNAME       VARCHAR(20),
 SHIPADDRESS    VARCHAR(20),
 SHIPCITY       VARCHAR(20),
 SHIPREGION     VARCHAR(20),
 SHIPPOSTALCODE VARCHAR(20),
 SHIPCOUNTRY    VARCHAR(20))
AS 
BEGIN
    for select CustomerID, CompanyName, ContactName, Address,
               City, Region, PostalCode,  Country, Phone, Fax,
               CONTACTTITLE
	from Customers 
	where City = :city
    into :customerID, :companyName, :contactName,  :address,
         :city, :region, :postalCode, :country, :phone, :fax,
         :CONTACTTITLE 
    do suspend;
    for select OrderID, CustomerID, EmployeeID, OrderDate, RequiredDate,
               ShippedDate, Shipvia, Freight, ShipName, ShipAddress, ShipCity,
               ShipRegion, ShipPostalCode, ShipCountry
    from Orders
    where CustomerID = :customerID 
    into :orderID, :customerID, :employeeID, :orderDate, :requiredDate,
         :shippedDate, :shipvia, :freight, :shipName, :shipAddress, :shipCity,
         :shipRegion, :shipPostalCode, :shipCountry
    do suspend;
END;";
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
                conn.Close();
            }
        }

        //????

        [Function]
        public new int AddCategory(
            [Parameter]string name,
            [Parameter]string description)
        {
            var result = ExecuteMethodCall(this, ((MethodInfo)(MethodBase.GetCurrentMethod())),
                                                   name, description).ReturnValue;
            return (int)result;
        }

        [Function]
        public int GetCustomersCountByRegion([Parameter]string region)
        {
            var result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), region);
            return ((int)(result.ReturnValue));
        }

        [Function]
        public ISingleResult<PartialCustomersSetResult> GetCustomersByCity(
            [Parameter] string city
             )
        {
            IExecuteResult result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), city);
            return ((ISingleResult<PartialCustomersSetResult>)(result.ReturnValue));
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

        [Function]
        [ResultType(typeof(Customer))]
        [ResultType(typeof(PartialCustomersSetResult))]
        public IMultipleResults SingleRowset_MultiShape(
            [Parameter] int? param)
        {
            IExecuteResult result = ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), param);
            return ((IMultipleResults)(result.ReturnValue));
        }

        [Function(Name = "GetCustomerAndOrders")]
        [ResultType(typeof(Customer))]
        [ResultType(typeof(Order))]
        public IMultipleResults GetCustomerAndOrders(
            [Parameter] string customerID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), customerID);
            return ((IMultipleResults)(result.ReturnValue));
        }
    }
}
