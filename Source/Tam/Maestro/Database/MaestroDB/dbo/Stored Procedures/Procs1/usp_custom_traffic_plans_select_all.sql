CREATE PROCEDURE usp_custom_traffic_plans_select_all
AS
SELECT
	*
FROM
	custom_traffic_plans WITH(NOLOCK)
