-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/17/2011
-- Description:	
-- =============================================
-- usp_MCS_GetDisplayMaterials_HouseIscisMissingClientIscis
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_HouseIscisMissingClientIscis]
AS
BEGIN
    SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		dm.is_house_isci=1
		AND dm.client_material_id IS NULL
		AND dm.type<>'MARRIED'
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
			AND m.is_house_isci=1
			AND m.real_material_id IS NULL
	ORDER BY
		mv.original_material_id,
		mv.ordinal
END
