Create PROCEDURE SingleRowset_MultiShape (param INT)
BEGIN 
        If param = 1 Then
            Select CustomerID, ContactName, CompanyName 
            From Customers;
        End If;
        
        If param = 2 Then
            Select * From Customers;
        End If;
END";