CREATE PROCEDURE usp_traffic_categories_update
(
	@id		Int,
	@name		VarChar(63)
)
AS
UPDATE traffic_categories SET
	name = @name
WHERE
	id = @id

