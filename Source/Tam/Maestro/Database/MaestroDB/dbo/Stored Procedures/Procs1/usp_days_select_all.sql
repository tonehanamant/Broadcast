CREATE PROCEDURE usp_days_select_all
AS
SELECT
	*
FROM
	days WITH(NOLOCK)
