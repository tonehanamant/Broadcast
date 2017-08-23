CREATE PROCEDURE [dbo].[usp_broadcast_proposal_audit_log_delete]
(
	@id Int)
AS
DELETE FROM broadcast_proposal_audit_log WHERE id=@id

