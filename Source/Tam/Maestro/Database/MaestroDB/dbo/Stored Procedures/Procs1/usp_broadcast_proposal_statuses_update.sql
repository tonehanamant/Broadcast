CREATE PROCEDURE [dbo].[usp_broadcast_proposal_statuses_update]
(
	@id		TinyInt,
	@code		VarChar(15),
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE broadcast_proposal_statuses SET
	code = @code,
	name = @name,
	is_default = @is_default
WHERE
	id = @id


