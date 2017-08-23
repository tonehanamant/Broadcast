
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_ForMediaMonthAndCompany]
	@media_month_id INT,
	@product_ids VARCHAR(MAX)
AS
BEGIN

	DECLARE @product_ids_split TABLE (id INT);
	INSERT INTO @product_ids_split
		SELECT id FROM dbo.SplitIntegers(@product_ids);

	DECLARE @start_date AS DATETIME
	DECLARE @end_date AS DATETIME
 
	SELECT @start_date=start_date, @end_date=end_date FROM media_months WHERE id=@media_month_id

	-- find all material_id's matching company and month (including the components of marriend spots)
	CREATE TABLE #tmp (material_id INT)
	INSERT INTO #tmp
		SELECT DISTINCT
			m.id
		FROM
			materials m				(NOLOCK)
			LEFT JOIN @product_ids_split pids
				ON pids.id = m.product_id
		WHERE
			m.id IN (
				SELECT 
					DISTINCT original_material_id 
				FROM 
					material_revisions mr	(NOLOCK)
					JOIN materials m1		(NOLOCK) ON m1.id=mr.revised_material_id
					JOIN materials m2		(NOLOCK) ON m2.id=mr.original_material_id
					JOIN @product_ids_split pids
						on pids.id = m1.product_id or pids.id = m2.product_id
				WHERE 
					(@media_month_id <= 0 OR (m2.date_created BETWEEN @start_date 
					AND @end_date OR m2.date_received BETWEEN @start_date 
					AND @end_date) OR (m1.date_created BETWEEN @start_date 
					AND @end_date OR m1.date_received BETWEEN @start_date AND @end_date))
			)
			OR
			(
				(@media_month_id <= 0 OR (m.date_created BETWEEN @start_date 
				AND @end_date OR m.date_received BETWEEN @start_date AND @end_date))
				AND pids.id = m.product_id
				AND m.id NOT IN (SELECT mr.revised_material_id FROM material_revisions mr (NOLOCK))
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
