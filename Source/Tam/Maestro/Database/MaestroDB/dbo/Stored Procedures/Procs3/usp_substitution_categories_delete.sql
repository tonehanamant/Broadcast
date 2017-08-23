CREATE PROCEDURE usp_substitution_categories_delete
(
	@id Int
)
AS
DELETE FROM substitution_categories WHERE id=@id
