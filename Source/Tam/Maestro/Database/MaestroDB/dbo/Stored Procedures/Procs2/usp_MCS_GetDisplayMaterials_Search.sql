
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_Search]
	@search_text VARCHAR(MAX)
AS
BEGIN
	-- find all material_id's (including the components of marriend spots)
	CREATE TABLE #tmp (material_id INT)
	INSERT INTO #tmp
		SELECT DISTINCT
			dm.material_id
		FROM
			uvw_display_materials dm
		WHERE
			dm.code LIKE @search_text
			OR dm.title LIKE @search_text
			OR dm.product LIKE @search_text
			OR dm.advertiser LIKE @search_text
			OR dm.client_material_code LIKE @search_text
			
		UNION

		SELECT DISTINCT
			revised_material_id
		FROM
			material_revisions mr	(NOLOCK)
			JOIN materials m		(NOLOCK) ON m.id=mr.original_material_id
			LEFT JOIN products p	(NOLOCK) ON p.id=m.product_id 
			JOIN uvw_display_materials dm ON dm.material_id=m.id
		WHERE
			dm.code LIKE @search_text
			OR dm.title LIKE @search_text
			OR dm.product LIKE @search_text
			OR dm.advertiser LIKE @search_text
			OR dm.client_material_code LIKE @search_text
			
		UNION

		SELECT DISTINCT
			original_material_id
		FROM
			material_revisions mr	(NOLOCK)
			JOIN materials m		(NOLOCK) ON m.id=mr.revised_material_id
			LEFT JOIN products p	(NOLOCK) ON p.id=m.product_id 
			JOIN uvw_display_materials dm ON dm.material_id=m.id
		WHERE
			dm.code LIKE @search_text
			OR dm.title LIKE @search_text
			OR dm.product LIKE @search_text
			OR dm.advertiser LIKE @search_text
			OR dm.client_material_code LIKE @search_text

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
