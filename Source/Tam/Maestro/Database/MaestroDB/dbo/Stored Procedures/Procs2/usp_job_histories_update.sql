CREATE PROCEDURE usp_job_histories_update
(
	@job_id		Int,
	@start_date		DateTime,
	@type		VarChar(15),
	@priority		Int,
	@status		VarChar(15),
	@active		Bit,
	@end_date		DateTime
)
AS
UPDATE job_histories SET
	start_date = @start_date,
	type = @type,
	priority = @priority,
	active = @active
WHERE
	job_id = @job_id AND
	status = @status AND
	end_date = @end_date
