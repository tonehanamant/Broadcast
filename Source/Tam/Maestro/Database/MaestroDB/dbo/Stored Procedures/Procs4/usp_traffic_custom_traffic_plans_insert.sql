CREATE PROCEDURE usp_traffic_custom_traffic_plans_insert
(
	@traffic_id		Int,
	@custom_traffic_plan_id		Int
)
AS
INSERT INTO traffic_custom_traffic_plans
(
	traffic_id,
	custom_traffic_plan_id
)
VALUES
(
	@traffic_id,
	@custom_traffic_plan_id
)

