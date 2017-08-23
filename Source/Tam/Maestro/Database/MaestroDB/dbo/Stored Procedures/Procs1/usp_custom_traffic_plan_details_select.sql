CREATE PROCEDURE usp_custom_traffic_plan_details_select
(
	@network_id		Int,
	@custom_traffic_plan_id		Int
)
AS
SELECT
	*
FROM
	custom_traffic_plan_details WITH(NOLOCK)
WHERE
	network_id=@network_id
	AND
	custom_traffic_plan_id=@custom_traffic_plan_id

