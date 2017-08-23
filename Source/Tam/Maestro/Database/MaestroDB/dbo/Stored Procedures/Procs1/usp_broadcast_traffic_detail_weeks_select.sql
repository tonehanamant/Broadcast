CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_weeks_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_traffic_detail_weeks WITH(NOLOCK)
WHERE
	id = @id

