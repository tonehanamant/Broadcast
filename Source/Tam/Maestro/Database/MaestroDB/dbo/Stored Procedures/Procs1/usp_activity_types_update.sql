CREATE PROCEDURE usp_activity_types_update
(
	@id		Int,
	@name		VarChar(20),
	@is_default		Bit
)
AS
UPDATE activity_types SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

