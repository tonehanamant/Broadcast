CREATE PROCEDURE usp_cmw_traffic_product_descriptions_update
(
	@id		Int,
	@product_description		VarChar(127)
)
AS
UPDATE cmw_traffic_product_descriptions SET
	product_description = @product_description
WHERE
	id = @id

