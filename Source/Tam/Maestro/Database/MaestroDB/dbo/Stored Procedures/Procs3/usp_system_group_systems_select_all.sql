CREATE PROCEDURE usp_system_group_systems_select_all
AS
SELECT
	*
FROM
	system_group_systems WITH(NOLOCK)
