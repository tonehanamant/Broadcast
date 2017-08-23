CREATE PROCEDURE usp_system_group_histories_select_all
AS
SELECT
	*
FROM
	system_group_histories WITH(NOLOCK)
