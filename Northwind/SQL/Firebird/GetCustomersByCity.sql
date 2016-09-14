--Procedure: GetCustomersByCity

--DROP PROCEDURE "GetCustomersByCity";

SET TERM ^ ;

RECREATE PROCEDURE "GetCustomersByCity" (
 CITY         VARCHAR(20))
RETURNS (
 P0           VARCHAR(50),
 P2           VARCHAR(50),
 P1           VARCHAR(50),
 RETURN_VALUE VARCHAR(50))
AS 
BEGIN
    for select "CustomerID", "ContactName", "CompanyName"
	from "Customers" 
	where "City" = :city
    into :p0, :p1, :p2 do
    suspend;
END^

SET TERM ; ^