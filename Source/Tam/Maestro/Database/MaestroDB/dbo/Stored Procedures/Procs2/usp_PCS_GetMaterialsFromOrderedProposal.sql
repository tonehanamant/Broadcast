-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2012
-- Description:	
-- =============================================
-- usp_PCS_GetMaterialsFromOrderedProposal 21093
CREATE PROCEDURE usp_PCS_GetMaterialsFromOrderedProposal
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT DISTINCT
		m.*
	FROM
		proposal_materials pm (NOLOCK)
		JOIN materials m (NOLOCK) ON m.id=pm.material_id
	WHERE
		pm.proposal_id=@proposal_id
		OR pm.proposal_id IN (
			SELECT id FROM proposals (NOLOCK) WHERE original_proposal_id=@proposal_id
		)
END
