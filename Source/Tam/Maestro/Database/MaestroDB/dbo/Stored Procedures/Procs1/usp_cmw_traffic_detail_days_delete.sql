CREATE PROCEDURE usp_cmw_traffic_detail_days_delete
(
	@cmw_traffic_details_id		Int,
	@day_id		Int)
AS
DELETE FROM
	cmw_traffic_detail_days
WHERE
	cmw_traffic_details_id = @cmw_traffic_details_id
 AND
	day_id = @day_id
