CREATE PROCEDURE usp_proposal_types_select_all
AS
SELECT
	*
FROM
	proposal_types WITH(NOLOCK)
