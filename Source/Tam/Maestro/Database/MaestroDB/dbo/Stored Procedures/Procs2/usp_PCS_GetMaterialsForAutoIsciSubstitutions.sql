-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/5/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetMaterialsForAutoIsciSubstitutions
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT DISTINCT
		m.*
	FROM
		proposal_materials pm (NOLOCK)
		JOIN materials m (NOLOCK) ON m.id=pm.material_id
	WHERE
		pm.proposal_id IN (
			SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		)
END
