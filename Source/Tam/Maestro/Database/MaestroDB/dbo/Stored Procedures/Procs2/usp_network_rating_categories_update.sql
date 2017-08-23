CREATE PROCEDURE usp_network_rating_categories_update
(
	@id		Int,
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
UPDATE network_rating_categories SET
	name = @name,
	description = @description
WHERE
	id = @id

