CREATE PROCEDURE usp_zone_state_histories_select
(
	@zone_id		Int,
	@state_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	zone_state_histories WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	state_id=@state_id
	AND
	start_date=@start_date

