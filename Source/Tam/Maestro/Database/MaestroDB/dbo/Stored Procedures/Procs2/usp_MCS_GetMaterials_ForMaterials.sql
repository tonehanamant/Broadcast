-- =============================================
-- Author:		Stephen DeFusco
-- ALTER date:  8/23/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterials_ForMaterials]
	@original_material_id INT
AS
BEGIN
    SELECT	
		m.*
	FROM 
		material_revisions mr (NOLOCK)
		JOIN materials m (NOLOCK) ON m.id=mr.revised_material_id
	WHERE 
		mr.original_material_id=@original_material_id
	ORDER BY
		mr.ordinal
END
