CREATE PROCEDURE usp_daypart_days_select
(
	@daypart_id		Int,
	@day_id		Int
)
AS
SELECT
	*
FROM
	daypart_days WITH(NOLOCK)
WHERE
	daypart_id=@daypart_id
	AND
	day_id=@day_id

