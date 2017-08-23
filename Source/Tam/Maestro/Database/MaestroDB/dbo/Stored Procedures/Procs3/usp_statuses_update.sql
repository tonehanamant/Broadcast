CREATE PROCEDURE usp_statuses_update
(
	@id		Int,
	@status_set		VarChar(15),
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
UPDATE statuses SET
	status_set = @status_set,
	name = @name,
	description = @description
WHERE
	id = @id

