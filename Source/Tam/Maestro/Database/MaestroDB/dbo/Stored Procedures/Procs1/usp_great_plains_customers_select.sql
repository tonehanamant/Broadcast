
CREATE PROCEDURE [dbo].[usp_great_plains_customers_select]
(
	@customer_number		Char(8)
)
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
  FROM [dbo].[great_plains_customers] WITH (NOLOCK)
WHERE
	customer_number=@customer_number


