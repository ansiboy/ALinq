Create PROCEDURE AddCategory (pName CHAR(15), pDescription Text)
BEGIN 
        Insert Into Categories ( CategoryName, Description )
        Values ( pName, pDescription );
END