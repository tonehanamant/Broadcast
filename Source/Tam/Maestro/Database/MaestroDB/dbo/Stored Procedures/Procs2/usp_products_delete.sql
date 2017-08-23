CREATE PROCEDURE usp_products_delete
(
	@id Int
)
AS
DELETE FROM products WHERE id=@id
