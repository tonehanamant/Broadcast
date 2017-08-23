
CREATE PROCEDURE [dbo].[usp_great_plains_mapping_select_all]
AS
SELECT [advertiser_id]
      ,[product_id]
      ,[cmw_division_id]
      ,[great_plains_customer_number]
      ,[advertiser_alias]
      ,[product_alias]
  FROM
	great_plains_mapping WITH(NOLOCK)

