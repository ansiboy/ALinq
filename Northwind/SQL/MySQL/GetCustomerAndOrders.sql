Create PROCEDURE GetCustomerAndOrders (pCustomerID Char(15))
BEGIN 
        Select * From Customers Where CustomerID = pCustomerID;
        Select * From Orders Where CustomerID = pCustomerID;
END