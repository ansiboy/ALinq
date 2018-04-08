## XML 映射

ALinq 支持外部映射，借助于该过程，可以使用单独的 XML 文件指定数据库的数据模型与对象模型之间的映射。 

使用外部映射文件具有以下优点：

* 可以将映射代码放在应用程序代码外部。 此方法可以降低应用程序代码的混乱程度。

* 可以将外部映射文件视为类似于配置文件的某种东西。 例如，在发布二进制文件后，只需交换出外部映射文件，就可以更新应用程序的工作方式。

* 方便程序支持多种数据库

**示例**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Database Name="Northwind" Provider="ALinq.Oracle.OracleProvider" xmlns="http://schemas.microsoft.com/linqtosql/mapping/2007">
  <Table Name="Users" Member="Users">
    <Type Name="NorthwindDemo.User">
      <Column Name="ID" Member="ID" Storage="_ID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" AutoSync="OnInsert" />
      <Column Name="Manager" Member="Manager" Storage="_Manager" DbType="VarChar(30)" CanBeNull="false" />
      <Column Name="Assistant" Member="Assistant" Storage="_Assistant" DbType="VarChar(30)" CanBeNull="false" />
      <Column Name="Department" Member="Department" Storage="_Department" DbType="VarChar(30)" CanBeNull="false" />
    </Type>
  </Table>
  <Table Name="SqlMethod" Member="SqlMethods">
    <Type Name="NorthwindDemo.SqlMethod">
      <Column Name="ID" Member="ID" Storage="_ID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" AutoSync="OnInsert" />
      <Column Name="Name" Member="Name" Storage="_Name" CanBeNull="false" />
    </Type>
  </Table>
  <Table Name="Contacts" Member="Contacts">
    <Type Name="NorthwindDemo.Contact" InheritanceCode="Unknown" IsInheritanceDefault="true">
      <Column Name="ContactID" Member="ContactID" Storage="_ContactID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" UpdateCheck="Never" AutoSync="OnInsert" />
      <Column Name="ContactType" Member="ContactType" Storage="_ContactType" DbType="VarChar(40)" CanBeNull="false" IsDiscriminator="true" UpdateCheck="Never" />
      <Column Name="CompanyName" Member="CompanyName" Storage="_CompanyName" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Phone" Member="Phone" Storage="_Phone" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="GUID" Member="GUID" Storage="_GUID" CanBeNull="false" />
      <Type Name="NorthwindDemo.FullContact" InheritanceCode="Full">
        <Column Name="ContactName" Member="ContactName" Storage="_ContactName" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Column Name="ContactTitle" Member="ContactTitle" Storage="_ContactTitle" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Column Name="Address" Member="Address" Storage="_Address" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Column Name="City" Member="City" Storage="_City" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Column Name="Region" Member="Region" Storage="_Region" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Column Name="PostalCode" Member="PostalCode" Storage="_PostalCode" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Column Name="Country" Member="Country" Storage="_Country" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Column Name="Fax" Member="Fax" Storage="_Fax" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        <Type Name="NorthwindDemo.EmployeeContact" InheritanceCode="EmployeeContact">
          <Column Name="PhotoPath" Member="PhotoPath" Storage="_PhotoPath" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
          <Column Name="Photo" Member="Photo" Storage="_Photo" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
          <Column Name="Extension" Member="Extension" Storage="_Extension" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        </Type>
        <Type Name="NorthwindDemo.SupplierContact" InheritanceCode="Supplier">
          <Column Name="HomePage" Member="HomePage" Storage="_HomePage" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
        </Type>
        <Type Name="NorthwindDemo.CustomerContact" InheritanceCode="Customer" />
      </Type>
      <Type Name="NorthwindDemo.ShipperContact" InheritanceCode="Shipper" />
    </Type>
  </Table>
  <Table Name="Categories" Member="Categories">
    <Type Name="NorthwindDemo.Category">
      <Column Name="CategoryID" Member="CategoryID" Storage="_CategoryID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" AutoSync="OnInsert" />
      <Column Name="CategoryName" Member="CategoryName" Storage="_CategoryName" DbType="VarChar(15)" CanBeNull="false"  UpdateCheck="Never"/>
      <Column Name="Description" Member="Description" Storage="_Description" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Picture" Member="Picture" Storage="_Picture" UpdateCheck="Never" />
      <Association Name="Category_Product" Member="Products" Storage="_Products" ThisKey="CategoryID" OtherKey="CategoryID" />
    </Type>
  </Table>
  <Table Name="CustomerCustomerDemo" Member="CustomerCustomerDemos">
    <Type Name="NorthwindDemo.CustomerCustomerDemo">
      <Column Name="CustomerID" Member="CustomerID" Storage="_CustomerID" DbType="VarChar(5)" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="CustomerTypeID" Member="CustomerTypeID" Storage="_CustomerTypeID" DbType="VarChar(10)" CanBeNull="false" IsPrimaryKey="true" />
      <Association Name="CD_CCD" Member="CustomerDemographic" Storage="_CustomerDemographic" ThisKey="CustomerTypeID" OtherKey="CustomerTypeID" IsForeignKey="true" />
      <Association Name="C_CCD" Member="Customer" Storage="_Customer" ThisKey="CustomerID" OtherKey="CustomerID" IsForeignKey="true" />
    </Type>
  </Table>
  <Table Name="CustomerDemographics" Member="CustomerDemographics">
    <Type Name="NorthwindDemo.CustomerDemographic">
      <Column Name="CustomerTypeID" Member="CustomerTypeID" Storage="_CustomerTypeID" DbType="VarChar(10)" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="CustomerDesc" Member="CustomerDesc" Storage="_CustomerDesc" CanBeNull="false" />
      <Association Name="CD_CCD" Member="CustomerCustomerDemos" Storage="_CustomerCustomerDemos" ThisKey="CustomerTypeID" OtherKey="CustomerTypeID" />
    </Type>
  </Table>
  <Table Name="Customers" Member="Customers">
    <Type Name="NorthwindDemo.Customer">
      <Column Name="CustomerID" Member="CustomerID" Storage="_CustomerID" DbType="VarChar(5)" CanBeNull="false" IsPrimaryKey="true" UpdateCheck="Never" />
      <Column Name="CompanyName" Member="CompanyName" Storage="_CompanyName" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="ContactName" Member="ContactName" Storage="_ContactName" DbType="VarChar(30)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="ContactTitle" Member="ContactTitle" Storage="_ContactTitle" DbType="VarChar(30)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Address" Member="Address" Storage="_Address" DbType="VarChar(60)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="City" Member="City" Storage="_City" DbType="VarChar(15)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Region" Member="Region" Storage="_Region" DbType="VarChar(15)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="PostalCode" Member="PostalCode" Storage="_PostalCode" DbType="VarChar(10)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Country" Member="Country" Storage="_Country" DbType="VarChar(15)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Phone" Member="Phone" Storage="_Phone" DbType="VarChar(24)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Fax" Member="Fax" Storage="_Fax" DbType="VarChar(24)" CanBeNull="false" UpdateCheck="Never" />
      <Association Name="C_CCD" Member="CustomerCustomerDemos" Storage="_CustomerCustomerDemos" ThisKey="CustomerID" OtherKey="CustomerID" />
      <Association Name="Customer_Order" Member="Orders" Storage="_Orders" ThisKey="CustomerID" OtherKey="CustomerID" />
    </Type>
  </Table>
  <Table Name="EMPLOYEES" Member="Employees">
    <Type Name="NorthwindDemo.Employee">
      <Column Name="EmployeeID" Member="EmployeeID" Storage="_EmployeeID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" AutoSync="OnInsert" />
      <Column Name="LastName" Member="LastName" Storage="_LastName" DbType="VarChar(20)" CanBeNull="false" />
      <Column Name="FirstName" Member="FirstName" Storage="_FirstName" DbType="VarChar(10)" CanBeNull="false" />
      <Column Name="Title" Member="Title" Storage="_Title" DbType="VarChar(30)" />
      <Column Name="TitleOfCourtesy" Member="TitleOfCourtesy" Storage="_TitleOfCourtesy" DbType="VarChar(25)" />
      <Column Name="BirthDate" Member="BirthDate" Storage="_BirthDate" />
      <Column Name="HireDate" Member="HireDate" Storage="_HireDate" />
      <Column Name="Address" Member="Address" Storage="_Address" DbType="VarChar(60)" />
      <Column Name="City" Member="City" Storage="_City" DbType="VarChar(15)" />
      <Column Name="Region" Member="Region" Storage="_Region" DbType="VarChar(15)" />
      <Column Name="PostalCode" Member="PostalCode" Storage="_PostalCode" DbType="VarChar(10)" />
      <Column Name="Country" Member="Country" Storage="_Country" DbType="VarChar(15)" />
      <Column Name="HomePhone" Member="HomePhone" Storage="_HomePhone" DbType="VarChar(24)" />
      <Column Name="Extension" Member="Extension" Storage="_Extension" DbType="VarChar(4)" />
      <Column Name="Photo" Member="Photo" Storage="_Photo" UpdateCheck="Never" />
      <Column Name="Notes" Member="Notes" Storage="_Notes" />
      <Column Name="ReportsTo" Member="ReportsTo" Storage="_ReportsTo" />
      <Column Name="PhotoPath" Member="PhotoPath" Storage="_PhotoPath" DbType="VarChar(255)" />
      <Association Name="Employee_Employee" Member="Employees" Storage="_Employees" ThisKey="EmployeeID" OtherKey="ReportsTo" />
      <Association Name="Employee_EmployeeTerritory" Member="EmployeeTerritories" Storage="_EmployeeTerritories" ThisKey="EmployeeID" OtherKey="EmployeeID" />
      <Association Name="Employee_Order" Member="Orders" Storage="_Orders" ThisKey="EmployeeID" OtherKey="EmployeeID" />
      <Association Name="Employee_Employee" Member="ReportsToEmployee" Storage="_ReportsToEmployee" ThisKey="ReportsTo" OtherKey="EmployeeID" IsForeignKey="true" />
    </Type>
  </Table>
  <Table Name="EmployeeTerritories" Member="EmployeeTerritories">
    <Type Name="NorthwindDemo.EmployeeTerritory">
      <Column Name="EmployeeID" Member="EmployeeID" Storage="_EmployeeID" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="TerritoryID" Member="TerritoryID" Storage="_TerritoryID" DbType="VarChar(20)" CanBeNull="false" IsPrimaryKey="true" />
      <Association Name="Employee_EmployeeTerritory" Member="Employee" Storage="_Employee" ThisKey="EmployeeID" OtherKey="EmployeeID" IsForeignKey="true" />
      <Association Name="Territory_EmployeeTerritory" Member="Territory" Storage="_Territory" ThisKey="TerritoryID" OtherKey="TerritoryID" IsForeignKey="true" />
    </Type>
  </Table>
  <Table Name="OrderDetails" Member="OrderDetails">
    <Type Name="NorthwindDemo.OrderDetail">
      <Column Name="OrderID" Member="OrderID" Storage="_OrderID" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="ProductID" Member="ProductID" Storage="_ProductID" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="UnitPrice" Member="UnitPrice" Storage="_UnitPrice" CanBeNull="false" />
      <Column Name="Quantity" Member="Quantity" Storage="_Quantity" CanBeNull="false" />
      <Column Name="Discount" Member="Discount" Storage="_Discount" CanBeNull="false" />
      <Association Name="Order_OrderDetail" Member="Order" Storage="_Order" ThisKey="OrderID" OtherKey="OrderID" IsForeignKey="true" />
      <Association Name="Product_OrderDetail" Member="Product" Storage="_Product" ThisKey="ProductID" OtherKey="ProductID" IsForeignKey="true" />
    </Type>
  </Table>
  <Table Name="Orders" Member="Orders">
    <Type Name="NorthwindDemo.Order">
      <Column Name="OrderID" Member="OrderID" Storage="_OrderID" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="CustomerID" Member="CustomerID" Storage="_CustomerID" DbType="VarChar(5)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="EmployeeID" Member="EmployeeID" Storage="_EmployeeID" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="OrderDate" Member="OrderDate" Storage="_OrderDate" UpdateCheck="Never" />
      <Column Name="RequiredDate" Member="RequiredDate" Storage="_RequiredDate" UpdateCheck="Never" />
      <Column Name="ShippedDate" Member="ShippedDate" Storage="_ShippedDate" UpdateCheck="Never" />
      <Column Name="ShipVia" Member="ShipVia" Storage="_ShipVia" UpdateCheck="Never" />
      <Column Name="Freight" Member="Freight" Storage="_Freight" UpdateCheck="Never" />
      <Column Name="ShipName" Member="ShipName" Storage="_ShipName" DbType="VarChar(40)" UpdateCheck="Never" />
      <Column Name="ShipAddress" Member="ShipAddress" Storage="_ShipAddress" DbType="VarChar(60)" UpdateCheck="Never" />
      <Column Name="ShipCity" Member="ShipCity" Storage="_ShipCity" DbType="VarChar(15)" UpdateCheck="Never" />
      <Column Name="ShipRegion" Member="ShipRegion" Storage="_ShipRegion" DbType="VarChar(15)" UpdateCheck="Never" />
      <Column Name="ShipPostalCode" Member="ShipPostalCode" Storage="_ShipPostalCode" DbType="VarChar(10)" UpdateCheck="Never" />
      <Column Name="ShipCountry" Member="ShipCountry" Storage="_ShipCountry" DbType="VarChar(15)" UpdateCheck="Never" />
      <Association Name="Order_OrderDetail" Member="OrderDetails" Storage="_OrderDetails" ThisKey="OrderID" OtherKey="OrderID" />
      <Association Name="Customer_Order" Member="Customer" Storage="_Customer" ThisKey="CustomerID" OtherKey="CustomerID" IsForeignKey="true" />
      <Association Name="Employee_Order" Member="Employee" Storage="_Employee" ThisKey="EmployeeID" OtherKey="EmployeeID" IsForeignKey="true" />
      <Association Name="Shipper_Order" Member="Shipper" Storage="_Shipper" ThisKey="ShipVia" OtherKey="ShipperID" IsForeignKey="true" />
    </Type>
  </Table>
  <Table Name="Products" Member="Products">
    <Type Name="NorthwindDemo.Product">
      <Column Name="ProductID" Member="ProductID" Storage="_ProductID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" UpdateCheck="Never" AutoSync="OnInsert" />
      <Column Name="ProductName" Member="ProductName" Storage="_ProductName" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="SupplierID" Member="SupplierID" Storage="_SupplierID" UpdateCheck="Never" />
      <Column Name="CategoryID" Member="CategoryID" Storage="_CategoryID" UpdateCheck="Never" />
      <Column Name="QuantityPerUnit" Member="QuantityPerUnit" Storage="_QuantityPerUnit" DbType="VarChar(20)" UpdateCheck="Never" />
      <Column Name="UnitPrice" Member="UnitPrice" Storage="_UnitPrice" UpdateCheck="Never" />
      <Column Name="UnitsInStock" Member="UnitsInStock" Storage="_UnitsInStock" UpdateCheck="Never" />
      <Column Name="UnitsOnOrder" Member="UnitsOnOrder" Storage="_UnitsOnOrder" UpdateCheck="Never" />
      <Column Name="ReorderLevel" Member="ReorderLevel" Storage="_ReorderLevel" UpdateCheck="Never" />
      <Column Name="Discontinued" Member="Discontinued" Storage="_Discontinued" CanBeNull="false" UpdateCheck="Never" />
      <Association Name="Product_OrderDetail" Member="OrderDetails" Storage="_OrderDetails" ThisKey="ProductID" OtherKey="ProductID" />
      <Association Name="Category_Product" Member="Category" Storage="_Category" ThisKey="CategoryID" OtherKey="CategoryID" IsForeignKey="true" />
      <Association Name="Supplier_Product" Member="Supplier" Storage="_Supplier" ThisKey="SupplierID" OtherKey="SupplierID" IsForeignKey="true" />
    </Type>
  </Table>
  <Table Name="Region" Member="Regions">
    <Type Name="NorthwindDemo.Region">
      <Column Name="RegionID" Member="RegionID" Storage="_RegionID" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="RegionDescription" Member="RegionDescription" Storage="_RegionDescription" DbType="VarChar(50)" CanBeNull="false" />
      <Association Name="Region_Territory" Member="Territories" Storage="_Territories" ThisKey="RegionID" OtherKey="RegionID" />
    </Type>
  </Table>
  <Table Name="Shippers" Member="Shippers">
    <Type Name="NorthwindDemo.Shipper">
      <Column Name="ShipperID" Member="ShipperID" Storage="_ShipperID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" UpdateCheck="Never" AutoSync="OnInsert" />
      <Column Name="CompanyName" Member="CompanyName" Storage="_CompanyName" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Phone" Member="Phone" Storage="_Phone" DbType="VarChar(24)" CanBeNull="false" UpdateCheck="Never" />
      <Association Name="Shipper_Order" Member="Orders" Storage="_Orders" ThisKey="ShipperID" OtherKey="ShipVia" />
    </Type>
  </Table>
  <Table Name="Suppliers" Member="Suppliers">
    <Type Name="NorthwindDemo.Supplier">
      <Column Name="SupplierID" Member="SupplierID" Storage="_SupplierID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" AutoSync="OnInsert" />
      <Column Name="CompanyName" Member="CompanyName" Storage="_CompanyName" DbType="VarChar(40)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="ContactName" Member="ContactName" Storage="_ContactName" DbType="VarChar(30)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="ContactTitle" Member="ContactTitle" Storage="_ContactTitle" DbType="VarChar(30)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Address" Member="Address" Storage="_Address" DbType="VarChar(60)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="City" Member="City" Storage="_City" DbType="VarChar(15)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Region" Member="Region" Storage="_Region" DbType="VarChar(15)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="PostalCode" Member="PostalCode" Storage="_PostalCode" DbType="VarChar(10)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Country" Member="Country" Storage="_Country" DbType="VarChar(15)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Phone" Member="Phone" Storage="_Phone" DbType="VarChar(24)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="Fax" Member="Fax" Storage="_Fax" DbType="VarChar(24)" CanBeNull="false" UpdateCheck="Never" />
      <Column Name="HomePage" Member="HomePage" Storage="_HomePage" UpdateCheck="Never" />
      <Association Name="Supplier_Product" Member="Products" Storage="_Products" ThisKey="SupplierID" OtherKey="SupplierID" />
    </Type>
  </Table>
  <Table Name="Territories" Member="Territories">
    <Type Name="NorthwindDemo.Territory">
      <Column Name="TerritoryID" Member="TerritoryID" Storage="_TerritoryID" DbType="VarChar(20)" CanBeNull="false" IsPrimaryKey="true" />
      <Column Name="TerritoryDescription" Member="TerritoryDescription" Storage="_TerritoryDescription" DbType="VarChar(50)" CanBeNull="false" />
      <Column Name="RegionID" Member="RegionID" Storage="_RegionID" CanBeNull="false" />
      <Association Name="Territory_EmployeeTerritory" Member="EmployeeTerritories" Storage="_EmployeeTerritories" ThisKey="TerritoryID" OtherKey="TerritoryID" />
      <Association Name="Region_Territory" Member="Region" Storage="_Region" ThisKey="RegionID" OtherKey="RegionID" IsForeignKey="true" />
    </Type>
  </Table>
  <Table Name="TEMPS" Member="Temps">
    <Type Name="NorthwindDemo.Temp">
      <Column Name="ID" Member="ID" Storage="_ID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="true" UpdateCheck="Never" AutoSync="OnInsert" />
      <Column Name="IsA" Member="IsA" Storage="_IsA" />
      <Column Name="IsB" Member="IsB" Storage="_IsB" />
      <Column Name="Name" Member="Name" Storage="_Name" CanBeNull="false" DbType="VarChar(30)" />
    </Type>
  </Table>
  <Table Name="DataType" Member="DataTypes">
    <Type Name="NorthwindDemo.DataType">
      <Column Name="ID" Member="ID" Storage="_ID" CanBeNull="false" IsPrimaryKey="true" IsDbGenerated="false" AutoSync="Never" DbType="Number(20)" />
      <Column Name="Int64" Member="Int64" Storage="_Int64" CanBeNull="false" />
      <Column Name="UInt64" Member="UInt64" Storage="_UInt64" CanBeNull="false" />
      <Column Name="Enum" Member="Enum" Storage="_Enum" CanBeNull="false" />
      <Column Name="Char" Member="Char" Storage="_Char" CanBeNull="false" DbType="VarChar(40)"/>
      <Column Name="XDocument" Member="XDocument" Storage="_XDocument" CanBeNull="true" />
      <Column Name="XElement" Member="XElement" Storage="_XElement" CanBeNull="true" />
      <Column Name="Guid" Member="Guid" Storage="guid" CanBeNull="false"  IsDbGenerated="true" AutoSync="OnInsert"/>
    </Type>
  </Table>
 
</Database>
```