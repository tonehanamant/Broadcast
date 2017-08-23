CREATE PROCEDURE usp_network_traffic_dayparts_delete
(
	@nielsen_network_id		Int,
	@daypart_id		Int
)
AS
DELETE FROM network_traffic_dayparts WHERE nielsen_network_id=@nielsen_network_id AND daypart_id=@daypart_id
