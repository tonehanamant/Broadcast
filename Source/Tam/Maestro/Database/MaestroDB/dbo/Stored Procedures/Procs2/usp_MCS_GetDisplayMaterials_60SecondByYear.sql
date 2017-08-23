-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterials_60SecondByYear 2011
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_60SecondByYear]
	@year INT
AS
BEGIN
    SELECT 
		dm.*
	FROM 
		uvw_display_materials dm 
	WHERE
		dm.length=60
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
		JOIN materials m			(NOLOCK) ON m.id=mv.original_material_id
		LEFT JOIN spot_lengths sl	(NOLOCK) ON sl.id=m.spot_length_id
	WHERE
		sl.length=60
	ORDER BY
		mv.original_material_id,
		mv.ordinal
END
