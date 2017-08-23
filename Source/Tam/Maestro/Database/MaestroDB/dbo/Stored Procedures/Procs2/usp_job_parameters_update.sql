CREATE PROCEDURE usp_job_parameters_update
(
	@job_id		Int,
	@name		VarChar(50),
	@value		VarChar(1023)
)
AS
UPDATE job_parameters SET
	value = @value
WHERE
	job_id = @job_id AND
	name = @name
