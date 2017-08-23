CREATE PROCEDURE usp_categories_delete
(
	@id Int
)
AS
DELETE FROM categories WHERE id=@id
