CREATE PROCEDURE usp_proposal_details_select_all
AS
SELECT
	*
FROM
	proposal_details WITH(NOLOCK)
