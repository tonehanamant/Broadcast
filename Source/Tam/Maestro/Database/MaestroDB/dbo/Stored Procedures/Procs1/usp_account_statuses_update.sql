CREATE PROCEDURE usp_account_statuses_update
(
	@id		Int,
	@name		VarChar(127)
)
AS
UPDATE account_statuses SET
	name = @name
WHERE
	id = @id

