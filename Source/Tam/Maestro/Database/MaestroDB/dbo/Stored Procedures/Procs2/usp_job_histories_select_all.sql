CREATE PROCEDURE usp_job_histories_select_all
AS
SELECT
	*
FROM
	job_histories WITH(NOLOCK)
