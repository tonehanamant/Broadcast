CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_weeks_select_all]
AS
SELECT
	*
FROM
	broadcast_traffic_detail_weeks WITH(NOLOCK)

