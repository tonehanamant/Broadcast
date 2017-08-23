CREATE PROCEDURE usp_billing_cycles_update
(
	@id		Int,
	@name		VarChar(20),
	@is_default		Bit
)
AS
UPDATE billing_cycles SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

