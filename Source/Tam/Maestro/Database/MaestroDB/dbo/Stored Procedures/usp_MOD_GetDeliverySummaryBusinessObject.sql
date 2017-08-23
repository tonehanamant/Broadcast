-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/20/2014
-- Description:	
-- To do:		1) Group posts together (new feature)
--				2) Only include posts not specifically excluded from YTD's
--				3) Add MSA data source
--				4) Add clearance estimate data source
-- =============================================
-- usp_MOD_GetDeliverySummaryBusinessObject  1002354, 50610
CREATE PROCEDURE [dbo].[usp_MOD_GetDeliverySummaryBusinessObject]
	@tam_post_id INT,
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;
	CREATE TABLE #return (source VARCHAR(63), tam_post_id INT, tam_post_proposal_id INT, proposal_id INT, units FLOAT, rounded_units INT, subscribers BIGINT, hh_contracted_delivery FLOAT, demo_contracted_delivery FLOAT, contracted_total_cost MONEY, hh_delivery FLOAT, demo_delivery FLOAT, delivered_value MONEY)

    DECLARE @media_month_id INT;
    DECLARE @primary_audience_id INT;
    DECLARE @is_msa_post_available BIT;
    DECLARE @is_affidavit_post_available BIT;
    DECLARE @is_affidavit_fast_track_available BIT;
    DECLARE @is_post_log_fast_track_available BIT;
    DECLARE @is_post_log_daily_actuals_available BIT;
    
    -- meida month of posting plan
	SELECT
		@media_month_id = p.posting_media_month_id
	FROM
		dbo.proposals p (NOLOCK)
	WHERE
		p.id=@proposal_id;
	
	-- get primary demographic of post
	SELECT
		@primary_audience_id = MIN(pa.audience_id)
	FROM
		dbo.tam_posts tp					(NOLOCK)
		JOIN dbo.tam_post_proposals tpp		(NOLOCK) ON tpp.tam_post_id=tp.id
		LEFT JOIN dbo.proposal_audiences pa (NOLOCK) ON pa.proposal_id=tpp.posting_plan_proposal_id
			AND pa.ordinal=1
	WHERE
		tp.id=@tam_post_id
	GROUP BY
		tp.id;
			
    
    -- is msa post available
    SET @is_msa_post_available = 0
    
    -- is affidavit post available
    SELECT
		@is_affidavit_post_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	FROM
		dbo.tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id=@proposal_id
		AND tpp.is_fast_track=0
		AND tpp.post_completed IS NOT NULL
		AND tpp.aggregation_completed IS NOT NULL;
		
	-- is affidavit fast track available
	SELECT
		@is_affidavit_fast_track_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	FROM
		dbo.tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id=@proposal_id
		AND tpp.is_fast_track=1
		AND tpp.post_completed IS NOT NULL
		AND tpp.aggregation_completed IS NOT NULL;
	
	-- is post log fast track available	
	SELECT
		@is_post_log_fast_track_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	FROM
		postlog_staging.dbo.fast_tracks ft (NOLOCK)
	WHERE
		ft.posting_plan_proposal_id=@proposal_id
		AND ft.fast_track_run_id IN (
			SELECT MAX(ftr.id) FROM postlog_staging.dbo.fast_track_runs ftr (NOLOCK) WHERE ftr.media_month_id=@media_month_id AND ftr.date_completed IS NOT NULL AND ftr.run_type=1
		);
		
	-- is post log daily actuals available	
	SELECT
		@is_post_log_daily_actuals_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	FROM
		postlog_staging.dbo.daily_actuals da (NOLOCK)
	WHERE
		da.media_month_id=@media_month_id
		AND da.proposal_id=@proposal_id;
	
	
	SELECT
		@is_msa_post_available 'is_msa_post_available',
		@is_affidavit_post_available 'is_affidavit_post_available',
		@is_affidavit_fast_track_available 'is_affidavit_fast_track_available',
		@is_post_log_fast_track_available 'is_post_log_fast_track_available',
		@is_post_log_daily_actuals_available 'is_post_log_daily_actuals_available'
			
	
		
	IF @is_affidavit_post_available = 1
	BEGIN
		INSERT INTO #return
			SELECT
				'Affidavit Post' 'source',
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id,
				SUM(tpnd.units) 'dr_units',
				SUM(tpnd.rounded_units) 'dr_rounded_units',
				SUM(tpnd.subscribers) 'subscribers',
				CASE tp.is_equivalized
					WHEN 0 THEN
						SUM(c_hh.delivery)
					WHEN 1 THEN
						SUM(c_hh.eq_delivery)
				END 'contracted_hh_delivery',
				CASE tp.is_equivalized
					WHEN 0 THEN
						SUM(c_dm.delivery)
					WHEN 1 THEN
						SUM(c_dm.eq_delivery)
				END 'contracted_demo_delivery',
				SUM(c_hh.total_cost) 'contracted_total_cost',
				CASE tp.is_equivalized
					WHEN 0 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_hh.delivery) 
							WHEN 2 THEN SUM(tpnda_hh.dr_delivery) 
						END
					WHEN 1 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_hh.eq_delivery) 
							WHEN 2 THEN SUM(tpnda_hh.dr_eq_delivery) 
						END
				END 'hh_delivery',
				CASE tp.is_equivalized
					WHEN 0 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_dm.delivery) 
							WHEN 2 THEN SUM(tpnda_dm.dr_delivery) 
						END
					WHEN 1 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_dm.eq_delivery) 
							WHEN 2 THEN SUM(tpnda_dm.dr_eq_delivery) 
						END
				END 'demo_delivery',
				CASE tp.rate_card_type_id 
					WHEN 1 THEN SUM(
						(CASE tp.is_equivalized
							WHEN 0 THEN
								tpnda_dm.delivery
							WHEN 1 THEN
								tpnda_dm.eq_delivery
						END / 1000.0)
						*
						CASE tp.is_equivalized
							WHEN 0 THEN
								c_dm.cpm
							WHEN 1 THEN
								c_dm.eq_cpm
						END
					) 
					WHEN 2 THEN SUM(tpnd.rounded_units * c_hh.rate) 
				END 'delivered_value'
			FROM
				dbo.tam_posts tp (NOLOCK)
				JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
					AND tpp.posting_plan_proposal_id=@proposal_id
					AND tpp.tam_post_id=@tam_post_id
					AND tpp.is_fast_track=0
				LEFT JOIN dbo.tam_post_network_details tpnd (NOLOCK) ON tpnd.tam_post_proposal_id=tpp.id AND tpnd.[enabled]=1
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31)					c_hh ON c_hh.network_id=tpnd.network_id
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @primary_audience_id) c_dm ON c_dm.network_id=tpnd.network_id
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_hh (NOLOCK) ON tpnda_hh.tam_post_network_detail_id=tpnd.id AND tpnda_hh.audience_id=31
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_dm (NOLOCK) ON tpnda_dm.tam_post_network_detail_id=tpnd.id AND tpnda_dm.audience_id=@primary_audience_id
			GROUP BY
				tp.is_equivalized,
				tp.rate_card_type_id,
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id;
	END
	ELSE IF @is_affidavit_fast_track_available = 1
	BEGIN
		INSERT INTO #return
			SELECT
				'Affidavit Fast Track' 'source',
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id,
				SUM(tpnd.units) 'dr_units',
				SUM(tpnd.rounded_units) 'dr_rounded_units',
				SUM(tpnd.subscribers) 'subscribers',
				CASE tp.is_equivalized
					WHEN 0 THEN
						SUM(c_hh.delivery)
					WHEN 1 THEN
						SUM(c_hh.eq_delivery)
				END 'contracted_hh_delivery',
				CASE tp.is_equivalized
					WHEN 0 THEN
						SUM(c_dm.delivery)
					WHEN 1 THEN
						SUM(c_dm.eq_delivery)
				END 'contracted_demo_delivery',
				SUM(c_hh.total_cost) 'contracted_total_cost',
				CASE tp.is_equivalized
					WHEN 0 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_hh.delivery) 
							WHEN 2 THEN SUM(tpnda_hh.dr_delivery) 
						END
					WHEN 1 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_hh.eq_delivery) 
							WHEN 2 THEN SUM(tpnda_hh.dr_eq_delivery) 
						END
				END 'hh_delivery',
				CASE tp.is_equivalized
					WHEN 0 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_dm.delivery) 
							WHEN 2 THEN SUM(tpnda_dm.dr_delivery) 
						END
					WHEN 1 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(tpnda_dm.eq_delivery) 
							WHEN 2 THEN SUM(tpnda_dm.dr_eq_delivery) 
						END
				END 'demo_delivery',
				CASE tp.rate_card_type_id 
					WHEN 1 THEN SUM(
						(CASE tp.is_equivalized
							WHEN 0 THEN
								tpnda_dm.delivery
							WHEN 1 THEN
								tpnda_dm.eq_delivery
						END / 1000.0)
						*
						CASE tp.is_equivalized
							WHEN 0 THEN
								c_dm.cpm
							WHEN 1 THEN
								c_dm.eq_cpm
						END
					) 
					WHEN 2 THEN SUM(tpnd.rounded_units * c_hh.rate) 
				END 'delivered_value'
			FROM
				dbo.tam_posts tp (NOLOCK)
				JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
					AND tpp.tam_post_id=@tam_post_id
					AND tpp.posting_plan_proposal_id=@proposal_id
					AND tpp.is_fast_track=1
				JOIN dbo.tam_post_network_details tpnd (NOLOCK) ON tpnd.tam_post_proposal_id=tpp.id AND tpnd.[enabled]=1
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31)					c_hh ON c_hh.network_id=tpnd.network_id
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @primary_audience_id) c_dm ON c_dm.network_id=tpnd.network_id
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_hh (NOLOCK) ON tpnda_hh.tam_post_network_detail_id=tpnd.id AND tpnda_hh.audience_id=31
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_dm (NOLOCK) ON tpnda_dm.tam_post_network_detail_id=tpnd.id AND tpnda_dm.audience_id=@primary_audience_id
			GROUP BY
				tp.is_equivalized,
				tp.rate_card_type_id,
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id;
	END
	ELSE IF @is_post_log_fast_track_available = 1
	BEGIN
		DECLARE @latest_fast_track_run_id INT
		SELECT @latest_fast_track_run_id = MAX(ftr.id) FROM postlog_staging.dbo.fast_track_runs ftr (NOLOCK) WHERE ftr.media_month_id=@media_month_id AND ftr.date_completed IS NOT NULL AND ftr.run_type=1
		
		INSERT INTO #return
		SELECT
			'Post Log Fast Track' 'source',
			tp.id,
			tpp.id,
			tpp.posting_plan_proposal_id,
			SUM(
				CASE WHEN c_hh.universe > 0 THEN
					(ft.actual_subscribers + ft.gap_subscribers) / c_hh.universe
				ELSE
					0.0
			END) 'dr_units',
			ROUND(
				SUM(
					CASE WHEN c_hh.universe > 0 THEN
						(ft.actual_subscribers + ft.gap_subscribers) / c_hh.universe
					ELSE
						0.0
					END
			), 0) 'dr_rounded_units',
			SUM(ft.actual_subscribers + ft.gap_subscribers) 'subscribers',
			CASE tp.is_equivalized
					WHEN 0 THEN
						SUM(c_hh.delivery)
					WHEN 1 THEN
						SUM(c_hh.eq_delivery)
				END 'contracted_hh_delivery',
				CASE tp.is_equivalized
					WHEN 0 THEN
						SUM(c_dm.delivery)
					WHEN 1 THEN
						SUM(c_dm.eq_delivery)
				END 'contracted_demo_delivery',
				SUM(c_hh.total_cost) 'contracted_total_cost',
			CASE tp.is_equivalized
				WHEN 0 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(ft.actual_impressions + ft.gap_impressions) 
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(ft.actual_subscribers + ft.gap_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.delivery
							ELSE
								0.0
							END)
					END
				WHEN 1 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(ft.actual_impressions + ft.gap_impressions) * sl.delivery_multiplier
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(ft.actual_subscribers + ft.gap_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.eq_delivery
							ELSE
								0.0
							END)
					END
			END 'hh_delivery',
			CASE tp.is_equivalized
				WHEN 0 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(ft.actual_demo_impressions + ft.gap_demo_impressions)
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(ft.actual_subscribers + ft.gap_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.delivery
							ELSE
								0.0
							END)
					END
				WHEN 1 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(ft.actual_demo_impressions + ft.gap_demo_impressions) * sl.delivery_multiplier
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(ft.actual_subscribers + ft.gap_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.delivery * c_dm.vpvh
							ELSE
								0.0
							END)
					END
			END 'demo_delivery',
			CASE tp.rate_card_type_id 
				WHEN 1 THEN SUM(
					(CASE tp.is_equivalized
						WHEN 0 THEN
							ft.actual_demo_impressions + ft.gap_demo_impressions
						WHEN 1 THEN
							ft.actual_demo_impressions + ft.gap_demo_impressions
					END / 1000.0)
					*
					CASE tp.is_equivalized
						WHEN 0 THEN
							c_dm.cpm
						WHEN 1 THEN
							c_dm.eq_cpm
					END
				)
				WHEN 2 THEN SUM(CASE WHEN c_hh.universe > 0 THEN (ft.actual_subscribers + ft.gap_subscribers) / c_hh.universe ELSE 0.0 END * c_hh.rate) 
			END 'delivered_value'
		FROM
			dbo.tam_posts tp (NOLOCK)
			JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
				AND tpp.is_fast_track=0
			JOIN postlog_staging.dbo.fast_tracks ft (NOLOCK) ON ft.fast_track_run_id=@latest_fast_track_run_id 
				AND ft.posting_plan_proposal_id=tpp.posting_plan_proposal_id
			LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31) c_hh ON c_hh.network_id=ft.network_id
			LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @primary_audience_id) c_dm ON c_dm.network_id=ft.network_id
			JOIN dbo.proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
			JOIN dbo.spot_lengths sl (NOLOCK) ON sl.id=p.default_spot_length_id
		WHERE
			tp.is_deleted=0			-- posts that haven't been market deleted
			AND tp.post_type_code=1 -- posts that have been marked "Official"
		GROUP BY
			tp.id,
			tpp.id,
			tpp.is_fast_track,
			tpp.posting_plan_proposal_id,
			tp.is_equivalized,
			tp.rate_card_type_id,
			sl.delivery_multiplier
		ORDER BY
			tp.id,
			tpp.id;
	END	
	ELSE IF @is_post_log_daily_actuals_available = 1
	BEGIN		
		INSERT INTO #return
		SELECT
			'Post Log Daily Actuals' 'source',
			tp.id,
			tpp.id,
			tpp.posting_plan_proposal_id,
			SUM(
				CASE WHEN c_hh.universe > 0 THEN
					da.actual_subscribers / c_hh.universe
				ELSE
					0.0
			END) 'dr_units',
			ROUND(
				SUM(
					CASE WHEN c_hh.universe > 0 THEN
						da.actual_subscribers  / c_hh.universe
					ELSE
						0.0
					END
			), 0) 'dr_rounded_units',
			SUM(da.actual_subscribers) 'subscribers',
			CASE tp.is_equivalized
				WHEN 0 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(da.actual_hh_impressions) 
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(da.actual_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.delivery
							ELSE
								0.0
							END)
					END
				WHEN 1 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(da.actual_hh_impressions) * sl.delivery_multiplier
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(da.actual_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.eq_delivery
							ELSE
								0.0
							END)
					END
			END 'hh_delivery',
			CASE tp.is_equivalized
				WHEN 0 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(da.actual_demo_impressions)
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(da.actual_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.delivery
							ELSE
								0.0
							END)
					END
				WHEN 1 THEN
					CASE tp.rate_card_type_id 
						WHEN 1 THEN SUM(da.actual_demo_impressions) * sl.delivery_multiplier
						WHEN 2 THEN SUM(
							CASE WHEN c_hh.universe > 0 THEN
								(CAST(da.actual_subscribers AS FLOAT) / CAST(c_hh.universe / 1000.0 AS FLOAT)) * c_hh.delivery * c_dm.vpvh
							ELSE
								0.0
							END)
					END
			END 'demo_delivery',
			CASE tp.rate_card_type_id 
				WHEN 1 THEN SUM(
					(CASE tp.is_equivalized
						WHEN 0 THEN
							da.actual_demo_impressions
						WHEN 1 THEN
							da.actual_demo_impressions
					END / 1000.0)
					*
					CASE tp.is_equivalized
						WHEN 0 THEN
							c_dm.cpm
						WHEN 1 THEN
							c_dm.eq_cpm
					END
				)
				WHEN 2 THEN SUM(CASE WHEN c_hh.universe > 0 THEN (da.actual_subscribers) / c_hh.universe ELSE 0.0 END * c_hh.rate) 
			END 'delivered_value'
		FROM
			dbo.tam_posts tp (NOLOCK)
			JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
				AND tpp.is_fast_track=0
			JOIN postlog_staging.dbo.daily_actuals da (NOLOCK) ON da.media_month_id=@media_month_id
				AND da.proposal_id=tpp.posting_plan_proposal_id
			LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31) c_hh ON c_hh.network_id=da.network_id
			LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @primary_audience_id) c_dm ON c_dm.network_id=da.network_id
			JOIN dbo.proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
			JOIN dbo.spot_lengths sl (NOLOCK) ON sl.id=p.default_spot_length_id
		WHERE
			tp.is_deleted=0			-- posts that haven't been market deleted
			AND tp.post_type_code=1 -- posts that have been marked "Official"
		GROUP BY
			tp.id,
			tpp.id,
			tpp.is_fast_track,
			tpp.posting_plan_proposal_id,
			tp.is_equivalized,
			tp.rate_card_type_id,
			sl.delivery_multiplier
		ORDER BY
			tp.id,
			tpp.id;	
	END
	
	SELECT * FROM #return;
END