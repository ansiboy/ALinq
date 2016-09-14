--Procedure: SingleRowset_MultiShape

--DROP PROCEDURE "SingleRowset_MultiShape";

SET TERM ^ ;

RECREATE PROCEDURE "SingleRowset_MultiShape" (
 P0          INTEGER)
RETURNS (
 CUSTOMERID  VARCHAR(50),
 CONTACTNAME VARCHAR(50),
 COMPANYNAME VARCHAR(50),
 P4          VARCHAR(50),
 P5          VARCHAR(50),
 P6          VARCHAR(50),
 P7          VARCHAR(50),
 P8          VARCHAR(50),
 P9          VARCHAR(50),
 P10         VARCHAR(20))
AS 
BEGIN

if(:p0 = 1) then 
    for select "CustomerID", "ContactName", "CompanyName"
    from "Customers"
    where "Region" = 'WA'
    into :customerID, :ContactName, :CompanyName do
    suspend;
    
if(:p0 = 2) then
    for select "CustomerID", "ContactName", "CompanyName","Address",
               "City", "Region", "PostalCode", "Country", "Phone","Fax"
	from "Customers" 
	where "Region" = 'WA'
    into :customerID, :ContactName, :CompanyName, :P4, :p5, :p6, :p7, :p8, :p9, :p10 do
    suspend;
    
  
END^

SET TERM ; ^