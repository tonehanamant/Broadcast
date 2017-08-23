CREATE PROCEDURE [dbo].[usp_broadcast_proposal_employees_delete]
(
	@broadcast_proposal_id		Int,
	@employee_id		Int,
	@effective_date		DateTime)
AS
DELETE FROM
	broadcast_proposal_employees
WHERE
	broadcast_proposal_id = @broadcast_proposal_id
 AND
	employee_id = @employee_id
 AND
	effective_date = @effective_date

