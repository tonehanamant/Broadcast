CREATE PROCEDURE [dbo].[usp_broadcast_proposals_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_proposals WITH(NOLOCK)
WHERE
	id = @id

