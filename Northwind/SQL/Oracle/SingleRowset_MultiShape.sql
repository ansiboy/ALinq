create or replace package pkg2 is

type mytype is ref cursor;
procedure single_rowset_multi_shape(param in number,mycs out mytype,RETURN_VALUE out number);

end pkg2;


create or replace package body pkg2 is

procedure single_rowset_multi_shape(param in number,mycs out mytype,RETURN_VALUE out number) is
begin

if param = 1 then
     open mycs for select *
                   from Customers
                   where Region = 'WA';
end if;
                   
if param = 2 then
    open mycs for  select CustomerID, ContactName, CompanyName
                   from Customers 
                   where City = 'WA'; 
end if;  


                 
RETURN_VALUE := 1;                       
end single_rowset_multi_shape;

end pkg2;