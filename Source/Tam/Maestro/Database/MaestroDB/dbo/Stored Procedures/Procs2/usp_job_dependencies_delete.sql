CREATE PROCEDURE usp_job_dependencies_delete
(
	@predicate_job_id		Int,
	@dependent_job_id		Int)
AS
DELETE FROM
	job_dependencies
WHERE
	predicate_job_id = @predicate_job_id
 AND
	dependent_job_id = @dependent_job_id
