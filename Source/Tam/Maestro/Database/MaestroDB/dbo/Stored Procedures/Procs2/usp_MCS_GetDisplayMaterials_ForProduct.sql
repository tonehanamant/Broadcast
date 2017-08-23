-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterials_ForProduct 5278
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_ForProduct]
	@product_id INT
AS
BEGIN
	-- find all material_id's matching product (including the components of marriend spots)
	CREATE TABLE #tmp (material_id INT)
	INSERT INTO #tmp
		SELECT DISTINCT
			m.id
		FROM
			materials m (NOLOCK)
		WHERE
			m.product_id=@product_id
			OR
			m.id IN (
				SELECT 
					DISTINCT mv.original_material_id 
				FROM 
					material_revisions mv	(NOLOCK)
					JOIN materials m		(NOLOCK) ON m.id=mv.revised_material_id
				WHERE
					m.product_id=@product_id 
			)

    SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		dm.material_id IN (
			SELECT material_id FROM #tmp
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
			SELECT material_id FROM #tmp
		)
	ORDER BY
		mv.original_material_id,
		mv.ordinal

	DROP TABLE #tmp;
END
