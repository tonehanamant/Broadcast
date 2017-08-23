-- =============================================
-- Author:		Stephen DeFusco / Brenton Reeder
-- Create date: 2014/04/03
-- Description:	Gather the ISCI History
-- =============================================
-- usp_MCS_GetISCIHistoryReport 2398
CREATE PROCEDURE [dbo].[usp_MCS_GetISCIHistoryReport] 
	@material_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	-- 1) Lookup all "original ISCI's" for the selected ISCI (if any).
	SELECT 
		dm.* 
	FROM 
		uvw_display_materials dm
	WHERE 
		dm.client_material_id=@material_id
	
	-- 2) Lookup all "married ISCI's" for the selected ISCI (if any).
	SELECT
		dm.*
	FROM
		uvw_display_materials dm
			JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=dm.material_id AND mr.revised_material_id=@material_id
			
	-- 3) Lookup all reels associated for the selected ISCI and married instances of ISCI's.
	SELECT 
		r.*
	FROM 
		reels r (NOLOCK)
	WHERE
		r.id IN (
			SELECT rm.reel_id FROM reel_materials rm (NOLOCK) WHERE rm.material_id=@material_id
		) OR
		r.id IN (
			SELECT rm.reel_id FROM reel_materials rm (NOLOCK) JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=rm.material_id AND mr.revised_material_id=@material_id
		) 
	ORDER BY 
		r.name
		
	-- 4) Lookup all Traffic Orders for the selected ISCI and married instances of ISCI's.
	SELECT
		CASE WHEN t.original_traffic_id IS NULL THEN t.id ELSE t.original_traffic_id end 'id',
		t.id 'traffic_id',
		dbo.udf_GetTrafficSpotLength(t.id) 'length',
		CASE WHEN t.display_name IS NULL OR t.display_name = '' THEN
				t.name 
		  ELSE
				t.display_name 
		  END 'display_name',
		  dp.advertiser, 
		dp.product, 
		dbo.udf_GetTrafficFlightText(t.id) 'flight'
	FROM
		traffic t (NOLOCK)
		LEFT JOIN traffic_proposals tp (NOLOCK) on t.id = tp.traffic_id 
			AND tp.primary_proposal=1
		LEFT JOIN uvw_display_proposals dp (NOLOCK) ON dp.id=tp.proposal_id
	WHERE 
	  t.id IN (
		SELECT DISTINCT
			  t.id
		FROM
			  traffic t (NOLOCK)
			  JOIN traffic_materials tm (NOLOCK) ON tm.traffic_id=t.id
			  JOIN reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
				AND (
				  rm.material_id=@material_id
				  OR
				  rm.material_id IN (
					SELECT mr.original_material_id FROM material_revisions mr (NOLOCK) WHERE mr.revised_material_id=@material_id
				  )
				)
		)
	ORDER BY 
		t.name
		
	-- 5) Possibly lookup all instances where the selected ISCI was posted and report the posts they were in (tam_post_analysis_reports_isci_network_weeks).
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
	WHERE
		dp.id IN (
			SELECT DISTINCT
				tpp.tam_post_id
			FROM
				dbo.tam_post_analysis_reports_isci_network_weeks tparinw (NOLOCK)
					JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparinw.tam_post_proposal_id AND tpp.post_source_code=0
					JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id	AND tp.post_type_code=1
			WHERE
				tparinw.material_id=@material_id
		)
		
		
	-- 6) supporting married components (if applicable)
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
		mv.original_material_id IN (
			SELECT dm.material_id FROM uvw_display_materials dm WHERE  dm.client_material_id=@material_id
		)
		OR
		mv.original_material_id IN (
			SELECT dm.material_id FROM uvw_display_materials dm JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=dm.material_id AND mr.revised_material_id=@material_id
		)
	ORDER BY
		mv.original_material_id,
		mv.ordinal
		
		
	-- 7) posting plans
	SELECT
		dp.*
	FROM
		uvw_display_proposals dp
	WHERE
		dp.id IN (
			SELECT DISTINCT pm.proposal_id FROM proposal_materials pm (NOLOCK) WHERE pm.material_id=@material_id
		)
		
	UNION ALL
	
	SELECT
		dp.*
	FROM
		uvw_display_proposals dp
	WHERE
		dp.id IN (
			SELECT DISTINCT pm.proposal_id FROM proposal_materials pm (NOLOCK) JOIN material_revisions mr (NOLOCK) ON mr.original_material_id=pm.material_id WHERE mr.revised_material_id=@material_id
		)
END
