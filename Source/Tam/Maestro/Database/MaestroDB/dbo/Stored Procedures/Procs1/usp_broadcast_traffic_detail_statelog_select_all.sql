CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_statelog_select_all]
AS
SELECT
	*
FROM
	broadcast_traffic_detail_statelog WITH(NOLOCK)


