CREATE PROCEDURE usp_proposal_detail_worksheets_select_all
AS
SELECT
	*
FROM
	proposal_detail_worksheets WITH(NOLOCK)
