CREATE PROCEDURE usp_system_daypart_histories_select_all
AS
SELECT
	*
FROM
	system_daypart_histories WITH(NOLOCK)
