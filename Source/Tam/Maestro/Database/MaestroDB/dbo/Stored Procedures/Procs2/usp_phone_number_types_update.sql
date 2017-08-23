CREATE PROCEDURE usp_phone_number_types_update
(
	@id		Int,
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE phone_number_types SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

