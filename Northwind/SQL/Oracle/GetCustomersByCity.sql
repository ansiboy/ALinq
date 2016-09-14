create or replace package pkg1 is

type mytype is ref cursor;
procedure get_customers_by_city(city in VarChar, mycs out mytype, RETURN_VALUE out numeric);
  
end pkg1 ;


create or replace package body pkg1 is

procedure get_customers_by_city(city in VarChar, mycs out mytype, RETURN_VALUE out numeric) is
begin

open mycs for select CustomerID, ContactName, CompanyName
              from Customers
              where City = city;

RETURN_VALUE := 1;
end get_customers_by_city;

end pkg1;