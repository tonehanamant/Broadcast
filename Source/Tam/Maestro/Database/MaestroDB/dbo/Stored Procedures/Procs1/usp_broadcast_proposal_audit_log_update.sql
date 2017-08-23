CREATE PROCEDURE [dbo].[usp_broadcast_proposal_audit_log_update]
(
	@id		Int,
	@broadcast_proposal_id		Int,
	@employee_id		Int,
	@message		VarChar(255)
)
AS
UPDATE broadcast_proposal_audit_log SET
	broadcast_proposal_id = @broadcast_proposal_id,
	employee_id = @employee_id,
	message = @message
WHERE
	id = @id


