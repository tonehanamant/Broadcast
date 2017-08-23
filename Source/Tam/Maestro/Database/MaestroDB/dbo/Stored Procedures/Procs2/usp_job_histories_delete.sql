CREATE PROCEDURE usp_job_histories_delete
(
	@job_id		Int,
	@status		VarChar(15),
	@end_date		DateTime)
AS
DELETE FROM
	job_histories
WHERE
	job_id = @job_id
 AND
	status = @status
 AND
	end_date = @end_date
