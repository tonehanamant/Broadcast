-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterials_ForMaterial 8654
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_ForMaterial]
	@original_material_id INT
AS
BEGIN
    SELECT
		dm.*
	FROM
		uvw_display_materials dm
		JOIN material_revisions mv (NOLOCK) ON mv.revised_material_id=dm.material_id
			AND mv.original_material_id=@original_material_id
	ORDER BY
		mv.ordinal
END
