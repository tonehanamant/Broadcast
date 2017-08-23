-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterialsByYear 2010
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterialsByYear]
	@year INT
AS
BEGIN
	SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		YEAR(dm.date_received)=@year OR YEAR(dm.date_created)=@year


	-- married components (if applicable)
	SELECT DISTINCT
		mv.original_material_id,
		mv.ordinal,
		dm.*
	FROM 
		uvw_display_materials dm
		JOIN material_revisions mv	(NOLOCK) ON mv.revised_material_id=dm.material_id
		JOIN materials m (NOLOCK) ON m.id=mv.original_material_id
			AND (YEAR(m.date_received)=@year OR YEAR(m.date_created)=@year)
	ORDER BY
		mv.original_material_id,
		mv.ordinal
END
