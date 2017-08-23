
CREATE PROCEDURE [dbo].[usp_great_plains_customers_select_all]
AS
SELECT [customer_number]
      ,[customer_name]
      ,[Address1]
      ,[Address2]
      ,[City]
      ,[State]
      ,[Zip]
      ,[email]
      ,[phone]
      ,[date_modified]
      ,[modified_by]
      ,[include_ISCI]
      ,[cmw_division_id]
  FROM
	great_plains_customers WITH(NOLOCK)

