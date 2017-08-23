CREATE PROCEDURE [dbo].[usp_broadcast_proposal_statuses_select]
(
	@id TinyInt
)
AS
SELECT
	*
FROM
	broadcast_proposal_statuses WITH(NOLOCK)
WHERE
	id = @id

