CREATE PROCEDURE usp_job_parameters_insert
(
	@job_id		Int,
	@name		VarChar(50),
	@value		VarChar(1023)
)
AS
INSERT INTO job_parameters
(
	job_id,
	name,
	value
)
VALUES
(
	@job_id,
	@name,
	@value
)

