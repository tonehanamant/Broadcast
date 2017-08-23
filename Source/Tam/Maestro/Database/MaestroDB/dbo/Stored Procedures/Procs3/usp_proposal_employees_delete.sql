CREATE PROCEDURE usp_proposal_employees_delete
(
	@proposal_id		Int,
	@employee_id		Int,
	@effective_date		DateTime)
AS
DELETE FROM
	proposal_employees
WHERE
	proposal_id = @proposal_id
 AND
	employee_id = @employee_id
 AND
	effective_date = @effective_date
