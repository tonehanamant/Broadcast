CREATE PROCEDURE usp_job_dependencies_select
(
	@predicate_job_id		Int,
	@dependent_job_id		Int
)
AS
SELECT
	*
FROM
	job_dependencies WITH(NOLOCK)
WHERE
	predicate_job_id=@predicate_job_id
	AND
	dependent_job_id=@dependent_job_id

