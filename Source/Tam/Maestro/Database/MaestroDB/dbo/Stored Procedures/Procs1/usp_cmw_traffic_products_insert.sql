CREATE PROCEDURE usp_cmw_traffic_products_insert
(
	@id		Int		OUTPUT,
	@cmw_traffic_advertisers_id		Int,
	@name		NVarChar(63)
)
AS
INSERT INTO cmw_traffic_products
(
	cmw_traffic_advertisers_id,
	name
)
VALUES
(
	@cmw_traffic_advertisers_id,
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

