CREATE PROCEDURE usp_cmw_traffic_product_descriptions_delete
(
	@id Int
)
AS
DELETE FROM cmw_traffic_product_descriptions WHERE id=@id
