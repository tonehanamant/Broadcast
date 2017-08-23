CREATE PROCEDURE usp_system_zone_histories_select
(
	@zone_id		Int,
	@system_id		Int,
	@start_date		DateTime,
	@type		VarChar(15)
)
AS
SELECT
	*
FROM
	system_zone_histories WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	system_id=@system_id
	AND
	start_date=@start_date
	AND
	type=@type

