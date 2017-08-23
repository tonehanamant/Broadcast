CREATE PROCEDURE usp_daypart_days_select_all
AS
SELECT
	*
FROM
	daypart_days WITH(NOLOCK)
