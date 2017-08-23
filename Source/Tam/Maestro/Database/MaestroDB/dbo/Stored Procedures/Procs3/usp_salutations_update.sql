CREATE PROCEDURE usp_salutations_update
(
	@id		Int,
	@name		VarChar(15),
	@is_default		Bit
)
AS
UPDATE salutations SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

