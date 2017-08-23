CREATE PROCEDURE usp_job_histories_select
(
	@job_id		Int,
	@status		VarChar(15),
	@end_date		DateTime
)
AS
SELECT
	*
FROM
	job_histories WITH(NOLOCK)
WHERE
	job_id=@job_id
	AND
	status=@status
	AND
	end_date=@end_date

