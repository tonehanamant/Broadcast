CREATE PROCEDURE usp_custom_traffic_plan_details_insert
(
	@network_id		Int,
	@custom_traffic_plan_id		Int,
	@traffic_factor		Float,
	@spot_yield_weight_factor		Float
)
AS
INSERT INTO custom_traffic_plan_details
(
	network_id,
	custom_traffic_plan_id,
	traffic_factor,
	spot_yield_weight_factor
)
VALUES
(
	@network_id,
	@custom_traffic_plan_id,
	@traffic_factor,
	@spot_yield_weight_factor
)

