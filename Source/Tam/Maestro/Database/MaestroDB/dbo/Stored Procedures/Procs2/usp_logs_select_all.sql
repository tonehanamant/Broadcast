CREATE PROCEDURE usp_logs_select_all
AS
SELECT
	*
FROM
	logs WITH(NOLOCK)
