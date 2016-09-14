create or replace package pkg3 is

type mytype1 is ref cursor;
type mytype2 is ref cursor;
procedure get_customer_and_orders(CustomerID in VarChar, mycs1 out mytype1, mycs2 out mytype2, RETURN_VALUE out numeric);

end pkg3;

create or replace package body pkg3 is

procedure get_customer_and_orders(customerID in VarChar, mycs1 out mytype1, mycs2 out mytype2, RETURN_VALUE out numeric) is
begin

open mycs1 for select *
               from Customers
               where CustomerID = customerID;
                   
open mycs2 for  select *
                from Orders 
                where CustomerID = customerID; 
  
           
RETURN_VALUE := 1;                       
end get_customer_and_orders;

end pkg3;