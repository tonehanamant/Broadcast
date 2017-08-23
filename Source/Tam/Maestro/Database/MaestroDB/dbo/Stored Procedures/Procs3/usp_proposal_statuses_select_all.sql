CREATE PROCEDURE usp_proposal_statuses_select_all
AS
SELECT
	*
FROM
	proposal_statuses WITH(NOLOCK)
