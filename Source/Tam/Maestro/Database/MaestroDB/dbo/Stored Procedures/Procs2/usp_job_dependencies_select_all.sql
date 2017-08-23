CREATE PROCEDURE usp_job_dependencies_select_all
AS
SELECT
	*
FROM
	job_dependencies WITH(NOLOCK)
