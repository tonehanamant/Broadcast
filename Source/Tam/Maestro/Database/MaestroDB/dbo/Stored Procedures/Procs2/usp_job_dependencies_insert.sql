CREATE PROCEDURE usp_job_dependencies_insert
(
	@predicate_job_id		Int,
	@dependent_job_id		Int
)
AS
INSERT INTO job_dependencies
(
	predicate_job_id,
	dependent_job_id
)
VALUES
(
	@predicate_job_id,
	@dependent_job_id
)

