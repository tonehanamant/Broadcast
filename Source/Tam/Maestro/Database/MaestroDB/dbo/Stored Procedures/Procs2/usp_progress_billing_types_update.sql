CREATE PROCEDURE usp_progress_billing_types_update
(
	@id		Int,
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE progress_billing_types SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

