create or replace procedure add_category(category_name in VarChar, 
            category_description in VarChar,RETURN_VALUE out int) is
begin

Insert Into Categories
     (CategoryID,CategoryName, Description)
Values (seq_categories.nextval, category_name, category_description);
Select seq_categories.currval into RETURN_VALUE From Dual;

end add_category;