CREATE PROCEDURE usp_email_types_update
(
	@id		Int,
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE email_types SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

