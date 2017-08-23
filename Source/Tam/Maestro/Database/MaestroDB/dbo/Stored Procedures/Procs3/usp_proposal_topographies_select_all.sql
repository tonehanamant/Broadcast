CREATE PROCEDURE usp_proposal_topographies_select_all
AS
SELECT
	*
FROM
	proposal_topographies WITH(NOLOCK)
