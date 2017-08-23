CREATE PROCEDURE usp_proposal_employees_select_all
AS
SELECT
	*
FROM
	proposal_employees WITH(NOLOCK)
