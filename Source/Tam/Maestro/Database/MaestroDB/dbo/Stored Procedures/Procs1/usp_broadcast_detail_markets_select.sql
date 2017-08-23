CREATE PROCEDURE [dbo].[usp_broadcast_detail_markets_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_detail_markets WITH(NOLOCK)
WHERE
	id = @id

