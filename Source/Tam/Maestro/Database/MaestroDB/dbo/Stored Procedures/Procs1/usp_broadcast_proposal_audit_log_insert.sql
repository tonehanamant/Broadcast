CREATE PROCEDURE [dbo].[usp_broadcast_proposal_audit_log_insert]
(
	@id		int		OUTPUT,
	@broadcast_proposal_id		Int,
	@employee_id		Int,
	@message		VarChar(255)
)
AS
INSERT INTO broadcast_proposal_audit_log
(
	broadcast_proposal_id,
	employee_id,
	message
)
VALUES
(
	@broadcast_proposal_id,
	@employee_id,
	@message
)

SELECT
	@id = SCOPE_IDENTITY()


