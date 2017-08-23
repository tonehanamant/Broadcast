-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/21/2011
-- Description:	Retrieves all HD materials by year.
-- =============================================
-- usp_MCS_GetDisplayMaterials_HdByYear 2010
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_HdByYear]
	@year INT
AS
BEGIN
	SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		dm.is_hd=1
		AND (YEAR(dm.date_received)=@year OR YEAR(dm.date_created)=@year)
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
			AND dm.is_hd=1
			AND (YEAR(dm.date_received)=@year OR YEAR(dm.date_created)=@year)
	ORDER BY
		mv.original_material_id,
		mv.ordinal
END
