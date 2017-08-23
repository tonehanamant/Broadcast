CREATE PROCEDURE usp_system_custom_traffic_histories_select_all
AS
SELECT
	*
FROM
	system_custom_traffic_histories WITH(NOLOCK)
