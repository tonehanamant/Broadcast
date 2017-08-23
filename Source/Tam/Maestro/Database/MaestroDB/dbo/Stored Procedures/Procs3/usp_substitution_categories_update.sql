CREATE PROCEDURE usp_substitution_categories_update
(
	@id		Int,
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
UPDATE substitution_categories SET
	name = @name,
	description = @description
WHERE
	id = @id

