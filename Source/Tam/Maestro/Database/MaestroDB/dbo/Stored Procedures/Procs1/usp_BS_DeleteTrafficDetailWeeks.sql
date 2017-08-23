
CREATE PROCEDURE usp_BS_DeleteTrafficDetailWeeks
(
	@broadcast_traffic_detail_id int
)
AS

delete from broadcast_traffic_detail_weeks where broadcast_traffic_detail_id = @broadcast_traffic_detail_id;


