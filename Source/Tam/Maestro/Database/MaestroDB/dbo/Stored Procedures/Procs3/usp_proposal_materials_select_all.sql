CREATE PROCEDURE usp_proposal_materials_select_all
AS
SELECT
	*
FROM
	proposal_materials WITH(NOLOCK)
