CREATE PROCEDURE usp_proposal_detail_audiences_select_all
AS
SELECT
	*
FROM
	proposal_detail_audiences WITH(NOLOCK)
