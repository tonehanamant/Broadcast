CREATE PROCEDURE usp_rating_categories_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63)
)
AS
UPDATE rating_categories SET
	code = @code,
	name = @name
WHERE
	id = @id
