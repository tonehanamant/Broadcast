-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/17/2011
-- Description:	
-- =============================================
-- usp_MCS_GetDisplayMaterials_IscisWithScreeners
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_IscisWithScreeners]
AS
BEGIN
    SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		dm.has_screener=1
	ORDER BY
		dm.code

	-- married components (if applicable)
	SELECT DISTINCT
		mv.original_material_id,
		mv.ordinal,
		dm.*
	FROM 
		uvw_display_materials dm
		JOIN material_revisions mv	(NOLOCK) ON mv.revised_material_id=dm.material_id
		JOIN materials m (NOLOCK) ON m.id=mv.original_material_id
			AND m.has_screener=1
	ORDER BY
		mv.original_material_id,
		mv.ordinal
END
