CREATE PROCEDURE usp_jobs_select_all
AS
SELECT
	*
FROM
	jobs WITH(NOLOCK)
