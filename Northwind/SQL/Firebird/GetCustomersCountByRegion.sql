--Procedure: GetCustomersCountByRegion

--DROP PROCEDURE "GetCustomersCountByRegion";

SET TERM ^ ;

RECREATE PROCEDURE "GetCustomersCountByRegion" (
 P1           VARCHAR(20))
RETURNS (
 RETURN_VALUE INTEGER)
AS 
BEGIN
     select count(*) from "Customers" 
     where "Region" = :P1 
     into :RETURN_VALUE;
END^

SET TERM ; ^