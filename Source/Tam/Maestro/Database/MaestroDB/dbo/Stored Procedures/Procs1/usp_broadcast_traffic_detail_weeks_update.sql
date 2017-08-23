CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_weeks_update]
(
	@id		Int,
	@broadcast_traffic_detail_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
UPDATE broadcast_traffic_detail_weeks SET
	broadcast_traffic_detail_id = @broadcast_traffic_detail_id,
	start_date = @start_date,
	end_date = @end_date,
	selected = @selected
WHERE
	id = @id

