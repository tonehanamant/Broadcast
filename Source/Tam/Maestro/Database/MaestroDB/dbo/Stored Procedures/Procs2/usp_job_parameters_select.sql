CREATE PROCEDURE usp_job_parameters_select
(
	@job_id		Int,
	@name		VarChar(50)
)
AS
SELECT
	*
FROM
	job_parameters WITH(NOLOCK)
WHERE
	job_id=@job_id
	AND
	name=@name

