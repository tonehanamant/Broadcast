CREATE PROCEDURE usp_release_product_descriptions_update
(
	@id		Int,
	@product_description		VarChar(63)
)
AS
UPDATE release_product_descriptions SET
	product_description = @product_description
WHERE
	id = @id

