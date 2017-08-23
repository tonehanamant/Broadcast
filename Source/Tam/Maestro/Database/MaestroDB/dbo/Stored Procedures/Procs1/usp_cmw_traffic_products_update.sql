CREATE PROCEDURE usp_cmw_traffic_products_update
(
	@id		Int,
	@cmw_traffic_advertisers_id		Int,
	@name		NVarChar(63)
)
AS
UPDATE cmw_traffic_products SET
	cmw_traffic_advertisers_id = @cmw_traffic_advertisers_id,
	name = @name
WHERE
	id = @id

