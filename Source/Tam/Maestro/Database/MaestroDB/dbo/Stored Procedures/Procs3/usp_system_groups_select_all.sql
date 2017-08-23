CREATE PROCEDURE usp_system_groups_select_all
AS
SELECT
	*
FROM
	system_groups WITH(NOLOCK)
