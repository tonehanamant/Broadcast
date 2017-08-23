CREATE PROCEDURE [dbo].[usp_broadcast_traffic_details_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_traffic_details WITH(NOLOCK)
WHERE
	id = @id

