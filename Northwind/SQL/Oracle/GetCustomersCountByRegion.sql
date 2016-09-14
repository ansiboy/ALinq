create or replace procedure get_customers_count_by_region (
       region in VarChar,  RETURN_VALUE out numeric) is
begin
    select Count(*) into RETURN_VALUE 
	from Customers 
	where Region = region;
end get_customers_count_by_region;