-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterialsByIds '3348'
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterialsByIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		dm.material_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)

	-- married components (if applicable)
	SELECT DISTINCT
		mv.original_material_id,
		mv.ordinal,
		dm.*
	FROM 
		uvw_display_materials dm
		JOIN material_revisions mv	(NOLOCK) ON mv.revised_material_id=dm.material_id
	WHERE
		mv.original_material_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
	ORDER BY
		mv.original_material_id,
		mv.ordinal
END
