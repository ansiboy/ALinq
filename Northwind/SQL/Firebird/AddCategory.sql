--Procedure: AddCategory

--DROP PROCEDURE "AddCategory";

SET TERM ^ ;

RECREATE PROCEDURE "AddCategory" (
 P1           VARCHAR(20),
 P2           VARCHAR(20))
RETURNS (
 RETURN_VALUE INTEGER)
AS 
BEGIN
  Insert Into "Categories" ("CategoryID", "CategoryName", "Description")
  Values (NEXT VALUE FOR Seq_Categories, :p1, :p2); 
  
  SELECT GEN_ID(Seq_Categories, 0) FROM RDB$DATABASE into RETURN_VALUE;
  SUSPEND ;
END^

SET TERM ; ^