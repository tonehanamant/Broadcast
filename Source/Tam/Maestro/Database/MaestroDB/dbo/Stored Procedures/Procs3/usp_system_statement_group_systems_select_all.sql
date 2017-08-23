CREATE PROCEDURE usp_system_statement_group_systems_select_all
AS
SELECT
	*
FROM
	system_statement_group_systems WITH(NOLOCK)
