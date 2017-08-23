CREATE PROCEDURE usp_system_daypart_histories_select
(
	@system_id		Int,
	@daypart_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	system_daypart_histories WITH(NOLOCK)
WHERE
	system_id=@system_id
	AND
	daypart_id=@daypart_id
	AND
	start_date=@start_date

