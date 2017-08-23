CREATE PROCEDURE usp_categories_update
(
	@id		Int,
	@category_set		VarChar(15),
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
UPDATE categories SET
	category_set = @category_set,
	name = @name,
	description = @description
WHERE
	id = @id

