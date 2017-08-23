CREATE PROCEDURE usp_system_statement_systems_select
(
	@system_statement_id		Int,
	@system_id		Int
)
AS
SELECT
	*
FROM
	system_statement_systems WITH(NOLOCK)
WHERE
	system_statement_id=@system_statement_id
	AND
	system_id=@system_id

