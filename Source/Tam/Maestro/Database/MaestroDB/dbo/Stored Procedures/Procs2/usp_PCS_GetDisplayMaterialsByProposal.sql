-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/10/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetDisplayMaterialsByProposal
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		dm.*
	FROM
		proposal_materials pm (NOLOCK)
		JOIN uvw_display_materials dm ON dm.material_id=pm.material_id
	WHERE
		pm.proposal_id=@proposal_id
END
