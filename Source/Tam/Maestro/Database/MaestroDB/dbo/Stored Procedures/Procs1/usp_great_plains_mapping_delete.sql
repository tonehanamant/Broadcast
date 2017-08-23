
CREATE PROCEDURE [dbo].[usp_great_plains_mapping_delete]
(
	@advertiser_id		Int,
	@product_id		Int,
	@cmw_division_id		Int)
AS
DELETE FROM
	great_plains_mapping
WHERE
	advertiser_id = @advertiser_id
 AND
	product_id = @product_id
 AND
	cmw_division_id = @cmw_division_id

