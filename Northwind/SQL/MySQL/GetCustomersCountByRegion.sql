Create PROCEDURE GetCustomersCountByRegion (pRegion CHAR(15), OUT RETURN_VALUE int)
BEGIN 
        Select Count(*) INTO RETURN_VALUE From Customers
        Where Region = pRegion;
END