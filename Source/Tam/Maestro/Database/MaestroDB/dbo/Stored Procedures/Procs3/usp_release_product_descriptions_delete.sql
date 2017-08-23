CREATE PROCEDURE usp_release_product_descriptions_delete
(
	@id Int
)
AS
DELETE FROM release_product_descriptions WHERE id=@id
