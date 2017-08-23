CREATE PROCEDURE [dbo].[usp_broadcast_proposal_employees_insert]
(
	@broadcast_proposal_id		Int,
	@employee_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO broadcast_proposal_employees
(
	broadcast_proposal_id,
	employee_id,
	effective_date
)
VALUES
(
	@broadcast_proposal_id,
	@employee_id,
	@effective_date
)

