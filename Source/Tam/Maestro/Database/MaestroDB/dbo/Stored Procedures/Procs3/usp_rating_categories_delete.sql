CREATE PROCEDURE usp_rating_categories_delete
(
	@id		Int)
AS
DELETE FROM
	rating_categories
WHERE
	id = @id
