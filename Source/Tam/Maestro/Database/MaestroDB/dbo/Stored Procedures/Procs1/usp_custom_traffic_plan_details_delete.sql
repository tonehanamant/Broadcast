CREATE PROCEDURE usp_custom_traffic_plan_details_delete
(
	@network_id		Int,
	@custom_traffic_plan_id		Int)
AS
DELETE FROM
	custom_traffic_plan_details
WHERE
	network_id = @network_id
 AND
	custom_traffic_plan_id = @custom_traffic_plan_id
