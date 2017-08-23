CREATE PROCEDURE usp_job_parameters_select_all
AS
SELECT
	*
FROM
	job_parameters WITH(NOLOCK)
