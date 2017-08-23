CREATE PROCEDURE usp_proposal_audiences_select_all
AS
SELECT
	*
FROM
	proposal_audiences WITH(NOLOCK)
