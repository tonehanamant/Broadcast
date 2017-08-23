CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_statelog_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_traffic_detail_statelog WITH(NOLOCK)
WHERE
	id = @id


