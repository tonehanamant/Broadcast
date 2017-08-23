	
	CREATE PROCEDURE [dbo].[usp_PCS_GetPotentialDisplayMaterialsNotInPostingPlans]
		@media_month_id INT
	AS
	BEGIN
		SET NOCOUNT ON;
	
		DECLARE @start_date DATETIME, @end_date DATETIME;
		SELECT @start_date=mm.start_date, @end_date=mm.end_date FROM media_months mm (NOLOCK) WHERE mm.id=@media_month_id;
	
		-- get all received affidavit materials
		CREATE TABLE #affidavit_materials (material_id INT)
		INSERT INTO #affidavit_materials
			SELECT DISTINCT a.material_id FROM affidavits a (NOLOCK) WHERE a.media_month_id=@media_month_id;
		-- get all received post_log materials
		CREATE TABLE #postlog_materials (material_id INT)
		INSERT INTO #postlog_materials
			SELECT DISTINCT pl.material_id FROM postlog_staging.dbo.post_logs pl (NOLOCK) WHERE pl.media_month_id=@media_month_id;
		-- get all trafficked materials in month
		CREATE TABLE #trafficked_materials (material_id INT)
		INSERT INTO #trafficked_materials
			SELECT DISTINCT
				rm.material_id
			FROM 
				traffic_materials tm (NOLOCK) 
				JOIN reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
			WHERE 
				tm.start_date <= @end_date AND tm.end_date >= @start_date;	
			
		CREATE TABLE #original_affidavit_materials (material_id INT)
		CREATE TABLE #original_postlog_materials (material_id INT)
		CREATE TABLE #original_traffic_materials (material_id INT)
	
		-- affidavit: non-married
		INSERT INTO #original_affidavit_materials
			SELECT
				m.id
			FROM
				#affidavit_materials am
				JOIN materials m (NOLOCK) ON m.id=am.material_id
					AND m.type='ORIGINAL'
				JOIN spot_lengths sl (NOLOCK) ON sl.id=m.spot_length_id;
		-- affidavit: married components
		INSERT INTO #original_affidavit_materials
			SELECT DISTINCT
				mrev.id
			FROM
				#affidavit_materials am
				JOIN materials m (NOLOCK) ON m.id=am.material_id
					AND m.type='MARRIED'
				JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=am.material_id
				JOIN materials mrev (NOLOCK) ON mrev.id=mr.revised_material_id
				JOIN spot_lengths sl (NOLOCK) ON sl.id=mrev.spot_length_id
			WHERE
				mrev.id NOT IN (
					SELECT material_id FROM #original_affidavit_materials
				);
		
		-- post_log: non-married
		INSERT INTO #original_postlog_materials
			SELECT
				m.id
			FROM
				#postlog_materials pm
				JOIN materials m (NOLOCK) ON m.id=pm.material_id
					AND m.type='ORIGINAL'
				JOIN spot_lengths sl (NOLOCK) ON sl.id=m.spot_length_id
			WHERE
				m.id NOT IN (
					SELECT material_id FROM #original_postlog_materials
				);
		-- post_log: married components
		INSERT INTO #original_postlog_materials
			SELECT DISTINCT
				mrev.id
			FROM
				#postlog_materials pm
				JOIN materials m (NOLOCK) ON m.id=pm.material_id
					AND m.type='MARRIED'
				JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=pm.material_id
				JOIN materials mrev (NOLOCK) ON mrev.id=mr.revised_material_id
				JOIN spot_lengths sl (NOLOCK) ON sl.id=mrev.spot_length_id
			WHERE
				mrev.id NOT IN (
					SELECT material_id FROM #original_postlog_materials
				);
	
		-- trafficked: non-married
		INSERT INTO #original_traffic_materials
			SELECT
				m.id
			FROM
				#trafficked_materials tm
				JOIN materials m (NOLOCK) ON m.id=tm.material_id
					AND m.type='ORIGINAL'
				JOIN spot_lengths sl (NOLOCK) ON sl.id=m.spot_length_id
			WHERE
				m.id NOT IN (
					SELECT material_id FROM #original_traffic_materials
				);
		-- trafficked: married components
		INSERT INTO #original_traffic_materials
			SELECT DISTINCT
				mrev.id
			FROM
				#trafficked_materials tm
				JOIN materials m (NOLOCK) ON m.id=tm.material_id
					AND m.type='MARRIED'
				JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=tm.material_id
				JOIN materials mrev (NOLOCK) ON mrev.id=mr.revised_material_id
				JOIN spot_lengths sl (NOLOCK) ON sl.id=mrev.spot_length_id
			WHERE
				mrev.id NOT IN (
					SELECT material_id FROM #original_traffic_materials
				);
	
	
		-- merge ORIGINAL (i.e. non-married) materials
		CREATE TABLE #original_materials (material_id INT, in_affidavits BIT, in_postlogs BIT, in_traffic BIT)
		INSERT INTO #original_materials
			SELECT material_id,0,0,0 FROM #original_affidavit_materials
			UNION
			SELECT material_id,0,0,0 FROM #original_postlog_materials
			UNION
			SELECT material_id,0,0,0 FROM #original_traffic_materials
	
		UPDATE
			#original_materials
		SET
			in_affidavits=CASE WHEN oam.material_id IS NULL THEN 0 ELSE 1 END,
			in_postlogs=CASE WHEN opm.material_id IS NULL THEN 0 ELSE 1 END,
			in_traffic=CASE WHEN otm.material_id IS NULL THEN 0 ELSE 1 END
		FROM
			#original_materials om
			LEFT JOIN #original_affidavit_materials oam ON oam.material_id=om.material_id
			LEFT JOIN #original_postlog_materials opm ON opm.material_id=om.material_id
			LEFT JOIN #original_traffic_materials otm ON otm.material_id=om.material_id
	
		SELECT 
			om.in_affidavits,
			om.in_postlogs,
			om.in_traffic,
			dm.*
		FROM 
			#original_materials om
			JOIN uvw_display_materials dm ON dm.material_id=om.material_id
		ORDER BY
			dm.code
	
		DROP TABLE #affidavit_materials;
		DROP TABLE #postlog_materials;
		DROP TABLE #trafficked_materials;
		DROP TABLE #original_materials;
	END
