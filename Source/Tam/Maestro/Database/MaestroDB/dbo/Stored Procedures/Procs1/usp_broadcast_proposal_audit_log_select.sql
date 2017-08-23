CREATE PROCEDURE [dbo].[usp_broadcast_proposal_audit_log_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_proposal_audit_log WITH(NOLOCK)
WHERE
	id = @id

