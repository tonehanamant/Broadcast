-- =============================================  
-- Author:  Stephen DeFusco  
-- Create date: 6/11/2010  
-- Description: <Description,,>  
-- =============================================  
-- usp_PCS_GetPostBusinessObject 2  
CREATE PROCEDURE [dbo].[usp_PCS_GetPostBusinessObject]  
	@tam_post_id INT,  
	@proposal_ids VARCHAR(MAX)  
AS  
BEGIN  
	CREATE TABLE #proposal_ids (proposal_id INT)  
	IF @proposal_ids IS NULL  
	BEGIN  
		INSERT INTO #proposal_ids  
			SELECT DISTINCT posting_plan_proposal_id FROM tam_post_proposals tpp (NOLOCK) WHERE tpp.tam_post_id=@tam_post_id  
	END  
	ELSE  
	BEGIN  
		INSERT INTO #proposal_ids  
			SELECT id FROM dbo.SplitIntegers(@proposal_ids)  
	END  

	-- post  
	SELECT  
		e_c.firstname + ' ' + e_c.lastname 'created_by',  
		e_m.firstname + ' ' + e_m.lastname 'modified_by',  
		e_l.firstname + ' ' + e_l.lastname 'locked_by',  
		dbo.ufn_generate_audit_string(phl.firstname, phl.lastname, phl.transmitted_date) AS media_ocean_last_sent,
		phl.result_status,
		tp.*,  
		rs.*  
	FROM  
		tam_posts tp (NOLOCK)  
		LEFT JOIN employees e_c (NOLOCK) ON e_c.id=tp.created_by_employee_id  
		LEFT JOIN employees e_m (NOLOCK) ON e_m.id=tp.modified_by_employee_id  
		LEFT JOIN employees e_l (NOLOCK) ON e_l.id=tp.locked_by_employee_id  
		JOIN rating_sources rs (NOLOCK) ON rs.id=tp.rating_source_id  
		LEFT JOIN uvw_post_tecc_log_latest phl (NOLOCK) ON tp.id = phl.tam_post_id
	WHERE  
		tp.id=@tam_post_id  
  
	-- ordered plan(s)  
	SELECT  
		dp.*  
	FROM   
		uvw_display_proposals dp  
	WHERE  
		dp.id IN (  
			SELECT DISTINCT original_proposal_id FROM proposals (NOLOCK) WHERE id IN (  
				SELECT proposal_id FROM #proposal_ids (NOLOCK)  
			)  
		)  
	ORDER BY   
		dp.id DESC  

	-- posting plans  
	SELECT  
		e.firstname + ' ' + e.lastname 'employee',  
		tpp.*,  
		dp.*  
	FROM  
		tam_post_proposals tpp (NOLOCK)  
		JOIN uvw_display_proposals dp ON dp.id=tpp.posting_plan_proposal_id  
		LEFT JOIN employees e (NOLOCK) ON e.id=tpp.posted_by_employee_id  
	WHERE  
		tpp.tam_post_id=@tam_post_id  
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)  
	ORDER BY  
		dp.start_date  

	-- materials in posting plans  
	SELECT  
		proposal_id,  
		material_id,  
		m.code  
	FROM  
		tam_post_proposals tpp (NOLOCK)  
		JOIN proposal_materials pm (NOLOCK) ON pm.proposal_id=tpp.posting_plan_proposal_id  
		JOIN materials m (NOLOCK) ON m.id=pm.material_id  
	WHERE  
		tpp.tam_post_id=@tam_post_id  
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)  

	-- substitute materials  
	SELECT  
		m.code + ' (' + CAST(sl.length AS VARCHAR(31)) + ')' 'material',  
		m_sub.code + ' (' + CAST(sl_sub.length AS VARCHAR(31)) + ')' 'substitute_material',  
		tpms.*  
	FROM  
		tam_post_material_substitutions tpms (NOLOCK)  
		JOIN materials m      (NOLOCK) ON m.id=tpms.material_id  
		JOIN materials m_sub     (NOLOCK) ON m_sub.id=tpms.substitute_material_id  
		JOIN spot_lengths sl     (NOLOCK) ON sl.id=m.spot_length_id  
		JOIN spot_lengths sl_sub    (NOLOCK) ON sl_sub.id=m_sub.spot_length_id  
	WHERE  
		tam_post_id=@tam_post_id  

	-- audiences  
	SELECT DISTINCT  
		pa.audience_id,   
		a.name 'audience',  
		pa.ordinal  
	FROM  
		tam_post_proposals tpp (NOLOCK)  
		JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=tpp.posting_plan_proposal_id  
		JOIN audiences a (NOLOCK) ON a.id=pa.audience_id  
	WHERE  
		tpp.tam_post_id=@tam_post_id  
		AND pa.ordinal>0  
	ORDER BY  
		pa.ordinal  

	-- report options  
	SELECT  
		post_buy_analysis_report_code  
	FROM  
		tam_post_report_options tpro (NOLOCK)  
	WHERE  
		tpro.tam_post_id=@tam_post_id  

	-- dayparts  
	SELECT  
		d.id,  
		d.code,  
		d.name,  
		d.start_time,  
		d.end_time,  
		d.mon,  
		d.tue,  
		d.wed,  
		d.thu,  
		d.fri,  
		d.sat,  
		d.sun  
	FROM  
		tam_post_dayparts tpd (NOLOCK)  
		JOIN vw_ccc_daypart d (NOLOCK) ON d.id=tpd.daypart_id  
	WHERE  
		tpd.tam_post_id=@tam_post_id  
	ORDER BY  
		tpd.ordinal  

	-- tam_post_gap_projections    
	SELECT  
		mm.*,  
		br.*,  
		dr.*  
	FROM  
		media_months mm (NOLOCK)  
		JOIN tam_post_gap_projections br (NOLOCK) ON br.media_month_id=mm.id AND br.rate_card_type_id=1 AND br.tam_post_id=@tam_post_id  
		JOIN tam_post_gap_projections dr (NOLOCK) ON dr.media_month_id=mm.id AND dr.rate_card_type_id=2 AND dr.tam_post_id=@tam_post_id  
	ORDER BY  
		mm.[year] DESC,  
		mm.[month] DESC  

	-- media months  
	SELECT DISTINCT  
		mm.*  
	FROM  
		tam_post_proposals tpp (NOLOCK)  
		JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id  
		JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id  
	WHERE  
		tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)  
	ORDER BY  
		mm.start_date  

	-- flights  
	SELECT  
		pf.*  
	FROM  
		proposal_flights pf (NOLOCK)  
	WHERE  
		pf.proposal_id IN (SELECT proposal_id FROM #proposal_ids)  

	-- tam_post_network_caps  
	SELECT  
		n.id,  
		n.code,  
		n.name,
		@tam_post_id,
		tpnc.network_delivery_cap_percentage,
		tpnc.bonus
	FROM   
		dbo.networks n (NOLOCK)  
		LEFT JOIN dbo.tam_post_network_caps tpnc (NOLOCK) ON tpnc.network_id = n.id   
		AND tpnc.tam_post_id = @tam_post_id  
	WHERE  
		n.id IN (  
			SELECT DISTINCT   
				pd.network_id   
			FROM   
				proposal_details pd (NOLOCK)  
			WHERE  
				pd.proposal_id IN (SELECT proposal_id FROM #proposal_ids)  
		)  
END  

