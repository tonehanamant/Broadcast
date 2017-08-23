CREATE PROCEDURE usp_zone_histories_select
(
	@zone_id		Int,
	@start_date		DateTime
)
AS
BEGIN
SELECT
	*
FROM
	zone_histories WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	start_date=@start_date

END
