-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	Find all reels for a given material (a.k.a. ISCI)
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_SerachReels_ByMaterialId]
	@material_id INT
AS
BEGIN
    SELECT	
		r.*
	FROM 
		reels r (NOLOCK)
	WHERE
		r.id IN (
			SELECT rm.reel_id FROM reel_materials rm (NOLOCK) WHERE rm.material_id=@material_id
		)
		OR
		r.id IN (
			SELECT rm.reel_id FROM reel_materials rm (NOLOCK) JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=rm.material_id AND mr.revised_material_id=@material_id
		) 
	ORDER BY 
		r.name
END

