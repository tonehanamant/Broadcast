CREATE PROCEDURE usp_system_custom_traffic_histories_select
(
	@system_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	system_custom_traffic_histories WITH(NOLOCK)
WHERE
	system_id=@system_id
	AND
	start_date=@start_date

