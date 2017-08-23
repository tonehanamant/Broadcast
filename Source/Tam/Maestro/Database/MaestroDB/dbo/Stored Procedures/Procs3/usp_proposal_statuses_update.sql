CREATE PROCEDURE usp_proposal_statuses_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE proposal_statuses SET
	code = @code,
	name = @name,
	is_default = @is_default
WHERE
	id = @id

