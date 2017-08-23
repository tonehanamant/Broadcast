CREATE PROCEDURE usp_system_statement_systems_delete
(
	@system_statement_id		Int,
	@system_id		Int)
AS
DELETE FROM
	system_statement_systems
WHERE
	system_statement_id = @system_statement_id
 AND
	system_id = @system_id
