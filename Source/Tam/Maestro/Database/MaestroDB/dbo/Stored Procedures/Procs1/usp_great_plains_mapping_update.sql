
CREATE PROCEDURE [dbo].[usp_great_plains_mapping_update]
(
	@advertiser_id		Int,
	@product_id		Int,
	@cmw_division_id		Int,
	@great_plains_customer_number		Char(8),
	@advertiser_alias		VarChar(63),
	@product_alias		VarChar(63)
)
AS
UPDATE great_plains_mapping SET
	great_plains_customer_number = @great_plains_customer_number,
	advertiser_alias = @advertiser_alias,
	product_alias = @product_alias
WHERE
	advertiser_id = @advertiser_id AND
	product_id = @product_id AND
	cmw_division_id = @cmw_division_id

