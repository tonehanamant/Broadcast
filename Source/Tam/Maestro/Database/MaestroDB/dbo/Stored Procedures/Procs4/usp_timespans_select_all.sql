CREATE PROCEDURE usp_timespans_select_all
AS
SELECT
	*
FROM
	timespans WITH(NOLOCK)
