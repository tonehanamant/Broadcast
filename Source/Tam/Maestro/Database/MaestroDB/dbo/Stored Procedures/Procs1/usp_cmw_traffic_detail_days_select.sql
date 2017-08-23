CREATE PROCEDURE usp_cmw_traffic_detail_days_select
(
	@cmw_traffic_details_id		Int,
	@day_id		Int
)
AS
SELECT
	*
FROM
	cmw_traffic_detail_days WITH(NOLOCK)
WHERE
	cmw_traffic_details_id=@cmw_traffic_details_id
	AND
	day_id=@day_id

