CREATE PROCEDURE usp_proposal_employees_select
(
	@proposal_id		Int,
	@employee_id		Int,
	@effective_date		DateTime
)
AS
SELECT
	*
FROM
	proposal_employees WITH(NOLOCK)
WHERE
	proposal_id=@proposal_id
	AND
	employee_id=@employee_id
	AND
	effective_date=@effective_date

