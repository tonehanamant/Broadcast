CREATE PROCEDURE usp_traffic_custom_traffic_plans_select
(
	@traffic_id		Int,
	@custom_traffic_plan_id		Int
)
AS
SELECT
	*
FROM
	traffic_custom_traffic_plans WITH(NOLOCK)
WHERE
	traffic_id=@traffic_id
	AND
	custom_traffic_plan_id=@custom_traffic_plan_id

