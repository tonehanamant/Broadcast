CREATE PROCEDURE usp_job_histories_insert
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
INSERT INTO job_histories
(
	job_id,
	start_date,
	type,
	priority,
	status,
	active,
	end_date
)
VALUES
(
	@job_id,
	@start_date,
	@type,
	@priority,
	@status,
	@active,
	@end_date
)

