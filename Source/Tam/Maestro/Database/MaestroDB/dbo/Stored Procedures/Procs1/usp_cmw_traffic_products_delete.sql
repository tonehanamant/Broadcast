CREATE PROCEDURE usp_cmw_traffic_products_delete
(
	@id Int
)
AS
DELETE FROM cmw_traffic_products WHERE id=@id
