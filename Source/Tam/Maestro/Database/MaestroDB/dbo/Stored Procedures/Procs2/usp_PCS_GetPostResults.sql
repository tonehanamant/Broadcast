
/* END products refactor */

/* BEGIN audiences refactor */
CREATE PROCEDURE [dbo].[usp_PCS_GetPostResults]
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
	
	DECLARE @effective_date DATETIME
	SELECT
		@effective_date = MIN(p.start_date)
	FROM
		proposals p (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=@tam_post_id AND tpp.posting_plan_proposal_id=p.id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)

	-- networks
	SELECT DISTINCT
		pd.network_id,
		CASE WHEN nu.code IS NULL THEN n.code ELSE nu.code END 'network'
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
		LEFT JOIN uvw_network_universe nu ON nu.network_id=pd.network_id AND (nu.start_date<=@effective_date AND (nu.end_date>=@effective_date OR nu.end_date IS NULL)) 
		LEFT JOIN uvw_network_universe n ON n.network_id=pd.network_id AND n.end_date IS NULL
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	ORDER BY
		CASE WHEN nu.code IS NULL THEN n.code ELSE nu.code END

	-- systems
	SELECT DISTINCT
		tpsd.business_id,
		tpsd.system_id,
		CASE WHEN bu.name IS NULL THEN b.name ELSE bu.name END 'affiliate',
		CASE WHEN su.code IS NULL THEN s.code ELSE su.code END 'system',
		CASE WHEN su.location IS NULL THEN s.location ELSE su.location END 'location'
	FROM
		tam_post_system_details tpsd (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		LEFT JOIN uvw_system_universe su ON su.system_id=tpsd.system_id AND (su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL))
		LEFT JOIN uvw_system_universe s ON s.system_id=tpsd.system_id AND s.end_date IS NULL
		LEFT JOIN uvw_business_universe bu ON bu.business_id=tpsd.business_id AND (bu.start_date<=@effective_date AND (bu.end_date>=@effective_date OR bu.end_date IS NULL))
		LEFT JOIN uvw_business_universe b ON b.business_id=tpsd.business_id AND b.end_date IS NULL
	WHERE
		tpp.tam_post_id=@tam_post_id
	ORDER BY
		CASE WHEN su.code IS NULL THEN s.code ELSE su.code END

	-- dmas
	SELECT
		d.dma_id,
		d.[rank],
		d.name,
		d.tv_hh,
		d.start_date 'effective_date'
	FROM
		uvw_dma_universe d
	WHERE
		d.start_date<=@effective_date AND (d.end_date>=@effective_date OR d.end_date IS NULL)
		AND d.dma_id<>211
	ORDER BY
		d.[rank]

-- Index = pSet.Tables[3]
	-- audiences
	SELECT DISTINCT
		pa.audience_id,
		pa.ordinal
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=tpp.posting_plan_proposal_id AND pa.ordinal>0
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	ORDER BY
		pa.ordinal

	-- materials (we union these tables because newer posts use the former and older the latter, we have to support both b/c we can't afford the post to "change" on us)
	SELECT DISTINCT
		tparinw.material_id,
		m.code,
		ISNULL(mc.code, '') 'client_code'
	FROM
		tam_post_analysis_reports_isci_network_weeks tparinw (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparinw.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		JOIN materials m (NOLOCK) ON m.id=tparinw.material_id
		LEFT JOIN materials mc (NOLOCK) ON mc.id=m.real_material_id
	WHERE
		tpp.tam_post_id=@tam_post_id

	UNION

	SELECT DISTINCT
		tparib.material_id,
		m.code,
		ISNULL(mc.code, '') 'client_code'
	FROM
		tam_post_analysis_reports_isci_breakdowns tparib (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparib.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		JOIN materials m (NOLOCK) ON m.id=tparib.material_id
		LEFT JOIN materials mc (NOLOCK) ON mc.id=m.real_material_id
	WHERE
		tpp.tam_post_id=@tam_post_id

	UNION

	SELECT DISTINCT
		tpms.substitute_material_id,
		m.code,
		ISNULL(mc.code, '') 'client_code'
	FROM
		tam_post_material_substitutions tpms (NOLOCK)
		JOIN materials m (NOLOCK) ON m.id=tpms.substitute_material_id
		LEFT JOIN materials mc (NOLOCK) ON mc.id=m.real_material_id
	WHERE
		tpms.tam_post_id=@tam_post_id

	-- media weeks
	SELECT DISTINCT
		tpp.posting_plan_proposal_id,
		tparinw.media_week_id,
		mw.start_date,
		mw.end_date,
		mw.week_number
	FROM
		tam_post_analysis_reports_isci_network_weeks tparinw (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparinw.tam_post_proposal_id 
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		JOIN media_weeks mw (NOLOCK) ON mw.id=tparinw.media_week_id
	WHERE
		tpp.tam_post_id=@tam_post_id
		
	UNION
	
	SELECT DISTINCT
		tpp.posting_plan_proposal_id,
		tparib.media_week_id,
		mw.start_date,
		mw.end_date,
		mw.week_number
	FROM
		tam_post_analysis_reports_isci_breakdowns tparib (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparib.tam_post_proposal_id 
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		JOIN media_weeks mw (NOLOCK) ON mw.id=tparib.media_week_id
	WHERE
		tpp.tam_post_id=@tam_post_id

	-- subscribers by system
	SELECT
		tpp.id,
		tpsd.business_id,
		tpsd.system_id,
		tpsd.network_id,
		tpsd.subscribers,
		tpsd.units
	FROM
		tam_post_system_details tpsd (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpsd.enabled=1

	-- subscribers by dma
	SELECT
		tpp.id,
		tpdd.dma_id,
		tpdd.network_id,
		tpdd.subscribers,
		tpdd.units
	FROM
		tam_post_dma_details tpdd (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpdd.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpdd.enabled=1

	-- delivery by system
	SELECT
		tpp.id,
		tpsd.business_id,
		tpsd.system_id,
		tpsd.network_id,
		tpsda.audience_id,
		tpsda.delivery,
		tpsda.eq_delivery,
		tpsda.dr_delivery,
		tpsda.dr_eq_delivery
	FROM
		tam_post_system_detail_audiences tpsda (NOLOCK)
		JOIN tam_post_system_details tpsd (NOLOCK) ON tpsd.id=tpsda.tam_post_system_detail_id
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpsd.enabled=1

	-- delivery by dma
	SELECT
		tpp.id,
		tpdd.dma_id,
		tpdd.network_id,
		tpdda.audience_id,
		tpdda.delivery,
		tpdda.eq_delivery,
		tpdda.dr_delivery,
		tpdda.dr_eq_delivery
	FROM
		tam_post_dma_detail_audiences tpdda (NOLOCK)
		JOIN tam_post_dma_details tpdd (NOLOCK) ON tpdd.id=tpdda.tam_post_dma_detail_id
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpdd.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpdd.enabled=1

	-- post setup lines
	SELECT
		pd.proposal_id,
		pd.id,
		sl.length,
		CASE WHEN nu.code IS NULL THEN n.code ELSE nu.code END 'network_code',
		pd.network_id,
		pd.num_spots 'units',
		pd.proposal_rate 'unit_rate',
		CAST(pd.proposal_rate * pd.num_spots AS MONEY) 'total_cost'
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
		LEFT JOIN uvw_network_universe nu ON nu.network_id=pd.network_id AND (nu.start_date<=@effective_date AND (nu.end_date>=@effective_date OR nu.end_date IS NULL)) 
		JOIN networks n ON n.id=pd.network_id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		AND tpp.post_source_code = 0
	ORDER BY
		pd.proposal_id,
		CASE WHEN nu.code IS NULL THEN n.code ELSE nu.code END

	-- post setup demographic data
	SELECT
		pd.proposal_id,
		pda.proposal_detail_id,
		pda.audience_id,
		pda.rating,
		pda.vpvh,
		pda.us_universe,
		pda.us_universe * pd.universal_scaling_factor 'coverage_universe',
		(pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0 'delivery',
		((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * CAST(pd.num_spots AS FLOAT) 'total_delivery',
		((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier 'eq_delivery',
		(((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier) * CAST(pd.num_spots AS FLOAT) 'eq_total_delivery',
		CAST(ISNULL(pd.proposal_rate / NULLIF( (pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0,0 ) ,0) AS MONEY) 'cpm',
		CAST(ISNULL(pd.proposal_rate / NULLIF( ((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0 ) * sl.delivery_multiplier,0) ,0) AS MONEY) 'eq_cpm'
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
		JOIN proposal_details pd (NOLOCK) ON tpp.posting_plan_proposal_id=pd.proposal_id
		JOIN proposal_detail_audiences pda (NOLOCK) ON pda.proposal_detail_id=pd.id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		AND tpp.post_source_code = 0

		-- post setup weekly details
	SELECT
		pd.proposal_id,
		pd.id,
		pdw.media_week_id,
		pdw.units,
		pd.proposal_rate 'unit_rate',
		pd.proposal_rate * pdw.units 'total_cost'
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
		JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		AND tpp.post_source_code = 0

	-- daypart analysis report
	SELECT
		tpp.id,
		tpard.network_id,
		tpard.audience_id,
		tpard.media_week_id,
		tpard.daypart_id,
		tpard.subscribers,
		tpard.delivery,
		tpard.eq_delivery,
		tpard.units,
		tpard.dr_delivery,
		tpard.dr_eq_delivery
	FROM
		tam_post_analysis_reports_dayparts tpard (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpard.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpard.enabled=1

	-- dma analysis report
	SELECT
		tpp.id,
		tpard.audience_id,

		tpard.subscribers_dma_1_10, 
		tpard.subscribers_dma_1_25, 
		tpard.subscribers_dma_1_50,  
		tpard.subscribers_dma_51_100,  
		tpard.subscribers_dma_101_210,  
		tpard.subscribers_dma_all,  

		tpard.delivery_dma_1_10,  
		tpard.delivery_dma_1_25,  
		tpard.delivery_dma_1_50,  
		tpard.delivery_dma_51_100,  
		tpard.delivery_dma_101_210,  
		tpard.delivery_dma_all,  

		tpard.eq_delivery_dma_1_10,  
		tpard.eq_delivery_dma_1_25,  
		tpard.eq_delivery_dma_1_50,  
		tpard.eq_delivery_dma_51_100,  
		tpard.eq_delivery_dma_101_210,  
		tpard.eq_delivery_dma_all,

		tpard.dr_delivery_dma_1_10,  
		tpard.dr_delivery_dma_1_25,  
		tpard.dr_delivery_dma_1_50,  
		tpard.dr_delivery_dma_51_100,  
		tpard.dr_delivery_dma_101_210,  
		tpard.dr_delivery_dma_all,

		tpard.dr_eq_delivery_dma_1_10,  
		tpard.dr_eq_delivery_dma_1_25,  
		tpard.dr_eq_delivery_dma_1_50,  
		tpard.dr_eq_delivery_dma_51_100,  
		tpard.dr_eq_delivery_dma_101_210,  
		tpard.dr_eq_delivery_dma_all,

		tpard.units_dma_1_10, 
		tpard.units_dma_1_25, 
		tpard.units_dma_1_50,  
		tpard.units_dma_51_100,  
		tpard.units_dma_101_210,  
		tpard.units_dma_all
	FROM
		tam_post_analysis_reports_dmas tpard (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpard.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpard.enabled=1
		
	-- dma analysis by network report
	SELECT
		tpp.id,
		tpardn.network_id,
		tpardn.audience_id,

		tpardn.subscribers_dma_1_10, 
		tpardn.subscribers_dma_1_25, 
		tpardn.subscribers_dma_1_50,  
		tpardn.subscribers_dma_51_100,  
		tpardn.subscribers_dma_101_210,  
		tpardn.subscribers_dma_all,  

		tpardn.delivery_dma_1_10,  
		tpardn.delivery_dma_1_25,  
		tpardn.delivery_dma_1_50,  
		tpardn.delivery_dma_51_100,  
		tpardn.delivery_dma_101_210,  
		tpardn.delivery_dma_all,  

		tpardn.eq_delivery_dma_1_10,  
		tpardn.eq_delivery_dma_1_25,  
		tpardn.eq_delivery_dma_1_50,  
		tpardn.eq_delivery_dma_51_100,  
		tpardn.eq_delivery_dma_101_210,  
		tpardn.eq_delivery_dma_all,

		tpardn.dr_delivery_dma_1_10,  
		tpardn.dr_delivery_dma_1_25,  
		tpardn.dr_delivery_dma_1_50,  
		tpardn.dr_delivery_dma_51_100,  
		tpardn.dr_delivery_dma_101_210,  
		tpardn.dr_delivery_dma_all,

		tpardn.dr_eq_delivery_dma_1_10,  
		tpardn.dr_eq_delivery_dma_1_25,  
		tpardn.dr_eq_delivery_dma_1_50,  
		tpardn.dr_eq_delivery_dma_51_100,  
		tpardn.dr_eq_delivery_dma_101_210,  
		tpardn.dr_eq_delivery_dma_all,

		tpardn.units_dma_1_10, 
		tpardn.units_dma_1_25, 
		tpardn.units_dma_1_50,  
		tpardn.units_dma_51_100,  
		tpardn.units_dma_101_210,  
		tpardn.units_dma_all
	FROM
		tam_post_analysis_reports_dma_networks tpardn (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpardn.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpardn.enabled=1
		
	-- isci breakdown report
	SELECT
		tpp.id,
		tparib.audience_id, 
		tparib.material_id, 
		tparib.media_week_id, 
		tparib.subscribers, 
		tparib.delivery, 
		tparib.eq_delivery,
		tparib.units,
		tparib.dr_delivery, 
		tparib.dr_eq_delivery
	FROM
		tam_post_analysis_reports_isci_breakdowns tparib (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparib.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tparib.enabled=1

	-- isci network analysis report
	SELECT
		tpp.id, 
		tparin.audience_id, 
		tparin.network_id, 
		tparin.material_id, 
		tparin.subscribers, 
		tparin.delivery, 
		tparin.eq_delivery,
		tparin.units,
		tparin.dr_delivery, 
		tparin.dr_eq_delivery
	FROM
		tam_post_analysis_reports_isci_networks tparin (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparin.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tparin.enabled=1

	-- spots per week report
	SELECT
		tpp.id, 
		tparspw.audience_id, 
		tparspw.network_id, 
		tparspw.media_week_id, 
		tparspw.subscribers, 
		tparspw.delivery, 
		tparspw.eq_delivery, 
		tparspw.units,
		tparspw.dr_delivery, 
		tparspw.dr_eq_delivery
	FROM
		tam_post_analysis_reports_spots_per_weeks tparspw (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparspw.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tparspw.enabled=1

	-- isci network week material analysis report
	SELECT
		tpp.id, 
		tparinw.audience_id, 
		tparinw.network_id, 
		tparinw.media_week_id,
		tparinw.material_id, 
		tparinw.subscribers, 
		tparinw.delivery, 
		tparinw.eq_delivery,
		tparinw.units,
		tparinw.dr_delivery, 
		tparinw.dr_eq_delivery
	FROM
		tam_post_analysis_reports_isci_network_weeks tparinw (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparinw.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tparinw.enabled=1

	-- dma grp/trp report
	SELECT
		tpp.id, 
		tpargtd.audience_id, 
		tpargtd.dma_id, 
		tpargtd.media_week_id,
		tpargtd.subscribers,
		tpargtd.delivery, 
		tpargtd.eq_delivery,
		tpargtd.units,
		tpargtd.dr_delivery, 
		tpargtd.dr_eq_delivery,
		tpargtd.grp,
		tpargtd.eq_grp,
		tpargtd.dr_grp,
		tpargtd.dr_eq_grp,
		tpargtd.trp,
		tpargtd.eq_trp,
		tpargtd.dr_trp,
		tpargtd.dr_eq_trp
	FROM
		tam_post_analysis_reports_grp_trp_dmas tpargtd (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpargtd.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpargtd.enabled=1

	-- us audiences
	SELECT
		pa.proposal_id,
		pa.audience_id,
		pa.universe
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=tpp.posting_plan_proposal_id
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		AND tpp.post_source_code = 0

	-- flights
	SELECT
		proposal_id,
		mw.id,
		mw.week_number,
		mm.month,
		pf.start_date,
		pf.end_date,
		CASE pf.selected WHEN 1 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END 'is_hiatus',
		mm.year
	FROM
		proposal_flights pf (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.posting_plan_proposal_id=pf.proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
			AND tpp.post_source_code = 0
		JOIN media_weeks mw (NOLOCK) ON (mw.start_date <= pf.end_date AND mw.end_date >= pf.start_date)
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		tpp.tam_post_id = @tam_post_id
	ORDER BY
		pf.start_date
		
	-- neq ratings per plan / demo
	SELECT DISTINCT
		tpp.posting_plan_proposal_id,
		pa.audience_id,
		dbo.GetProposalAudienceNEQRating(tpp.posting_plan_proposal_id, pa.audience_id) 'neq_rating'
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=tpp.posting_plan_proposal_id
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpp.post_source_code = 0
		
	-- media weeks delivering (any media week with 0 delivery implies there was no MIT data at the time it was posted/aggregated)
	-- (we union these two tables because newer posts use the former and older the latter, we have to support both b/c we can't afford the post to "change" on us)
	SELECT
		mm.media_month,
		mw.id,
		CASE 
			WHEN SUM(tparib.delivery) > 0.0 THEN 
				CAST(1 AS BIT) 
			ELSE 
				CAST(0 AS BIT) 
		END
	FROM
		tam_post_analysis_reports_isci_breakdowns tparib (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparib.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		JOIN media_weeks mw (NOLOCK) ON mw.id=tparib.media_week_id
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tparib.audience_id = 31
	GROUP BY
		mm.media_month,
		mw.id
		
	UNION
	
	SELECT DISTINCT
		mm.media_month,
		mw.id,
		CASE 
			WHEN SUM(tparinw.delivery) > 0.0 THEN 
				CAST(1 AS BIT) 
			ELSE 
				CAST(0 AS BIT) 
		END
	FROM
		tam_post_analysis_reports_isci_network_weeks tparinw (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparinw.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		JOIN media_weeks mw (NOLOCK) ON mw.id=tparinw.media_week_id
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tparinw.audience_id = 31
	GROUP BY
		mm.media_month,
		mw.id
		
	-- tam_post_materials
	SELECT
		tpari.tam_post_proposal_id, 
		tpari.material_id, 
		tpari.isci_material_id, 
		SUM(tpari.total_spots) 'total_spots',
		m.code
	FROM
		tam_post_analysis_reports_iscis tpari (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpari.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
		JOIN materials m (NOLOCK) ON m.id=tpari.isci_material_id
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpari.audience_id=31
		AND tpari.enabled=1
	GROUP BY
		tpari.tam_post_proposal_id, 
		tpari.material_id, 
		tpari.isci_material_id,
		m.code
	-- SELECT
	--	tpm.tam_post_proposal_id,
	--	tpm.material_id,
	--	tpm.affidavit_material_id,
	--	tpm.total_spots,
	--	m.code
	-- FROM
	--	tam_post_materials tpm (NOLOCK)
	--	JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpm.tam_post_proposal_id
	--		AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	--	JOIN materials m (NOLOCK) ON m.id=tpm.affidavit_material_id
		
	-- tam_post_analysis_reports_iscis
	SELECT
		tpari.tam_post_proposal_id, 
		tpari.audience_id, 
		tpari.network_id, 
		tpari.material_id, 
		tpari.isci_material_id, 
		tpari.subscribers, 
		tpari.delivery, 
		tpari.eq_delivery, 
		tpari.units, 
		tpari.dr_delivery, 
		tpari.dr_eq_delivery, 
		tpari.total_spots
	FROM
		tam_post_analysis_reports_iscis tpari (NOLOCK)
		JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpari.tam_post_proposal_id
			AND tpp.posting_plan_proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpari.enabled=1
		
	-- total us universes for use in daypart analysis pba trp calculation
	EXEC usp_PCS_GetTotalUsUniversesByPost @tam_post_id, @proposal_ids
				
	DROP TABLE #proposal_ids;
END
