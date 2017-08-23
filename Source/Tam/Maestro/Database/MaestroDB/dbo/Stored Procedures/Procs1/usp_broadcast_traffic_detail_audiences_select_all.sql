CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_audiences_select_all]
AS
SELECT
	*
FROM
	broadcast_traffic_detail_audiences WITH(NOLOCK)

