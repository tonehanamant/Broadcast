
CREATE PROCEDURE [dbo].[usp_great_plains_mapping_insert]
(
	@advertiser_id		Int,
	@product_id		Int,
	@cmw_division_id		Int,
	@great_plains_customer_number		Char(8),
	@advertiser_alias		VarChar(63),
	@product_alias		VarChar(63)
)
AS
INSERT INTO great_plains_mapping
(
	advertiser_id,
	product_id,
	cmw_division_id,
	great_plains_customer_number,
	advertiser_alias,
	product_alias
)
VALUES
(
	@advertiser_id,
	@product_id,
	@cmw_division_id,
	@great_plains_customer_number,
	@advertiser_alias,
	@product_alias
)


