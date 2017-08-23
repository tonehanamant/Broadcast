CREATE PROCEDURE usp_system_group_system_histories_select_all
AS
SELECT
	*
FROM
	system_group_system_histories WITH(NOLOCK)
