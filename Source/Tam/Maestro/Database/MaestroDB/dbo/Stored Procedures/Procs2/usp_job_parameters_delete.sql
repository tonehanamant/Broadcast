CREATE PROCEDURE usp_job_parameters_delete
(
	@job_id		Int,
	@name		VarChar(50))
AS
DELETE FROM
	job_parameters
WHERE
	job_id = @job_id
 AND
	name = @name
