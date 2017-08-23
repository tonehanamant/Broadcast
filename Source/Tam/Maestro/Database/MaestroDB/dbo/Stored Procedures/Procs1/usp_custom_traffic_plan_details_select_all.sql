CREATE PROCEDURE usp_custom_traffic_plan_details_select_all
AS
SELECT
	*
FROM
	custom_traffic_plan_details WITH(NOLOCK)
