CREATE PROCEDURE usp_traffic_custom_traffic_plans_select_all
AS
SELECT
	*
FROM
	traffic_custom_traffic_plans WITH(NOLOCK)
