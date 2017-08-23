CREATE PROCEDURE usp_proposal_types_update
(
	@id		Int,
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE proposal_types SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

