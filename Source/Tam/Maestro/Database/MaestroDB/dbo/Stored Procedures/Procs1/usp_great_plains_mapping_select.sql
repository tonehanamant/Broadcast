
CREATE PROCEDURE [dbo].[usp_great_plains_mapping_select]
(
	@advertiser_id		Int,
	@product_id		Int,
	@cmw_division_id		Int
)
AS
SELECT [advertiser_id]
      ,[product_id]
      ,[cmw_division_id]
      ,[great_plains_customer_number]
      ,[advertiser_alias]
      ,[product_alias]
  FROM
	great_plains_mapping WITH(NOLOCK)
WHERE
	advertiser_id=@advertiser_id
	AND
	product_id=@product_id
	AND
	cmw_division_id=@cmw_division_id


