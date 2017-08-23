CREATE PROCEDURE [dbo].[usp_broadcast_proposal_audit_log_select_all]
AS
SELECT
	*
FROM
	broadcast_proposal_audit_log WITH(NOLOCK)


