CREATE PROCEDURE usp_custom_traffic_plan_details_update
(
	@network_id		Int,
	@custom_traffic_plan_id		Int,
	@traffic_factor		Float,
	@spot_yield_weight_factor		Float
)
AS
UPDATE custom_traffic_plan_details SET
	traffic_factor = @traffic_factor,
	spot_yield_weight_factor = @spot_yield_weight_factor
WHERE
	network_id = @network_id AND
	custom_traffic_plan_id = @custom_traffic_plan_id
