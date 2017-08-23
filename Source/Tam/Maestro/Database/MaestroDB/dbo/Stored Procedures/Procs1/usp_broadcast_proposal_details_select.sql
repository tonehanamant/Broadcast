CREATE PROCEDURE [dbo].[usp_broadcast_proposal_details_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_proposal_details WITH(NOLOCK)
WHERE
	id = @id

