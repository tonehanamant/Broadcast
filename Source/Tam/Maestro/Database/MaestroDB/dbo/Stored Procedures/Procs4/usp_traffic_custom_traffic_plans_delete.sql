CREATE PROCEDURE usp_traffic_custom_traffic_plans_delete
(
	@traffic_id		Int,
	@custom_traffic_plan_id		Int)
AS
DELETE FROM
	traffic_custom_traffic_plans
WHERE
	traffic_id = @traffic_id
 AND
	custom_traffic_plan_id = @custom_traffic_plan_id
