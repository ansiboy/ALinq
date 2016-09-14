Create PROCEDURE GetCustomersByCity (pCity Char(30))
BEGIN 
        Select CustomerID, ContactName, CompanyName 
        From Customers
        Where Customers.City = pCity;
END