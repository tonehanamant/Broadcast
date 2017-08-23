CREATE PROCEDURE usp_cmw_traffic_detail_days_select_all
AS
SELECT
	*
FROM
	cmw_traffic_detail_days WITH(NOLOCK)
