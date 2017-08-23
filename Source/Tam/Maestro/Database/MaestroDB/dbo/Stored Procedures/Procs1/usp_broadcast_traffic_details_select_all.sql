CREATE PROCEDURE [dbo].[usp_broadcast_traffic_details_select_all]
AS
SELECT
	*
FROM
	broadcast_traffic_details WITH(NOLOCK)


