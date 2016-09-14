/*
public partial class User 
{
	public  ID
	{
	}
	public VarChar(30) Manager
	{
	}
	public VarChar(30) Assistant
	{
	}
	public VarChar(30) Department
	{
	}
}
public partial class SqlMethod 
{
	public  ID
	{
	}
	public  Name
	{
	}
}
public partial class Contact 
{
	public  ContactID
	{
	}
	public VarChar(40) ContactType
	{
	}
	public VarChar(40) CompanyName
	{
	}
	public VarChar(40) Phone
	{
	}
	public  GUID
	{
	}
}
public partial class FullContact 
{
	public VarChar(40) ContactName
	{
	}
	public VarChar(40) ContactTitle
	{
	}
	public VarChar(40) Address
	{
	}
	public VarChar(40) City
	{
	}
	public VarChar(40) Region
	{
	}
	public VarChar(40) PostalCode
	{
	}
	public VarChar(40) Country
	{
	}
	public VarChar(40) Fax
	{
	}
}
public partial class EmployeeContact 
{
	public VarChar(40) PhotoPath
	{
	}
	public VarChar(40) Photo
	{
	}
	public VarChar(40) Extension
	{
	}
}
public partial class SupplierContact 
{
	public VarChar(40) HomePage
	{
	}
}
public partial class CustomerContact 
{
}
public partial class ShipperContact 
{
}
public partial class Category 
{
	public  CategoryID
	{
	}
	public VarChar(15) CategoryName
	{
	}
	public  Description
	{
	}
	public  Picture
	{
	}
}
public partial class CustomerCustomerDemo 
{
	public VarChar(5) CustomerID
	{
	}
	public VarChar(10) CustomerTypeID
	{
	}
}
public partial class CustomerDemographic 
{
	public VarChar(10) CustomerTypeID
	{
	}
	public  CustomerDesc
	{
	}
}
public partial class Customer 
{
	public VarChar(5) CustomerID
	{
	}
	public VarChar(40) CompanyName
	{
	}
	public VarChar(30) ContactName
	{
	}
	public VarChar(30) ContactTitle
	{
	}
	public VarChar(60) Address
	{
	}
	public VarChar(15) City
	{
	}
	public VarChar(15) Region
	{
	}
	public VarChar(10) PostalCode
	{
	}
	public VarChar(15) Country
	{
	}
	public VarChar(24) Phone
	{
	}
	public VarChar(24) Fax
	{
	}
}
public partial class Employee 
{
	public  EmployeeID
	{
	}
	public VarChar(20) LastName
	{
	}
	public VarChar(10) FirstName
	{
	}
	public VarChar(30) Title
	{
	}
	public VarChar(25) TitleOfCourtesy
	{
	}
	public  BirthDate
	{
	}
	public  HireDate
	{
	}
	public VarChar(60) Address
	{
	}
	public VarChar(15) City
	{
	}
	public VarChar(15) Region
	{
	}
	public VarChar(10) PostalCode
	{
	}
	public VarChar(15) Country
	{
	}
	public VarChar(24) HomePhone
	{
	}
	public VarChar(4) Extension
	{
	}
	public  Photo
	{
	}
	public  Notes
	{
	}
	public  ReportsTo
	{
	}
	public VarChar(255) PhotoPath
	{
	}
}
public partial class EmployeeTerritory 
{
	public  EmployeeID
	{
	}
	public VarChar(20) TerritoryID
	{
	}
}
public partial class OrderDetail 
{
	public  OrderID
	{
	}
	public  ProductID
	{
	}
	public  UnitPrice
	{
	}
	public  Quantity
	{
	}
	public  Discount
	{
	}
}
public partial class Order 
{
	public  OrderID
	{
	}
	public VarChar(5) CustomerID
	{
	}
	public  EmployeeID
	{
	}
	public  OrderDate
	{
	}
	public  RequiredDate
	{
	}
	public  ShippedDate
	{
	}
	public  ShipVia
	{
	}
	public  Freight
	{
	}
	public VarChar(40) ShipName
	{
	}
	public VarChar(60) ShipAddress
	{
	}
	public VarChar(15) ShipCity
	{
	}
	public VarChar(15) ShipRegion
	{
	}
	public VarChar(10) ShipPostalCode
	{
	}
	public VarChar(15) ShipCountry
	{
	}
}
public partial class Product 
{
	public  ProductID
	{
	}
	public VarChar(40) ProductName
	{
	}
	public  SupplierID
	{
	}
	public  CategoryID
	{
	}
	public VarChar(20) QuantityPerUnit
	{
	}
	public  UnitPrice
	{
	}
	public  UnitsInStock
	{
	}
	public  UnitsOnOrder
	{
	}
	public  ReorderLevel
	{
	}
	public  Discontinued
	{
	}
}
public partial class Region 
{
	public  RegionID
	{
	}
	public VarChar(50) RegionDescription
	{
	}
}
public partial class Shipper 
{
	public  ShipperID
	{
	}
	public VarChar(40) CompanyName
	{
	}
	public VarChar(24) Phone
	{
	}
}
public partial class Supplier 
{
	public  SupplierID
	{
	}
	public VarChar(40) CompanyName
	{
	}
	public VarChar(30) ContactName
	{
	}
	public VarChar(30) ContactTitle
	{
	}
	public VarChar(60) Address
	{
	}
	public VarChar(15) City
	{
	}
	public VarChar(15) Region
	{
	}
	public VarChar(10) PostalCode
	{
	}
	public VarChar(15) Country
	{
	}
	public VarChar(24) Phone
	{
	}
	public VarChar(24) Fax
	{
	}
	public  HomePage
	{
	}
}
public partial class Territory 
{
	public VarChar(20) TerritoryID
	{
	}
	public VarChar(50) TerritoryDescription
	{
	}
	public  RegionID
	{
	}
}
public partial class Temp 
{
	public  ID
	{
	}
	public  IsA
	{
	}
	public  IsB
	{
	}
	public VarChar(30) Name
	{
	}
}
public partial class DataType 
{
	public  ID
	{
	}
	public  Int64
	{
	}
	public  UInt64
	{
	}
	public  Enum
	{
	}
	public Char(1) Char
	{
	}
	public  XDocument
	{
	}
	public  XElement
	{
	}
	public  Guid
	{
	}
}
public partial class Class1 
{
	public  ID1
	{
	}
	public  ID2
	{
	}
}
public partial class Class2 
{
	public  ID
	{
	}
	public  ID1
	{
	}
	public  ID2
	{
	}
}
*/