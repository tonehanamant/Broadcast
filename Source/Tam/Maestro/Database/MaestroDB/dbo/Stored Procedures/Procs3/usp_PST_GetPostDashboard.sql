-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/12/2012
-- Description:	Returns a summary of posts at the post level, proposal level, and network level
-- =============================================
-- usp_PST_GetPostDashboard 2015,1,1
CREATE PROCEDURE [dbo].[usp_PST_GetPostDashboard]
	@year INT,
	@quarter INT,
	@month INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #relavent_tam_post_proposal_ids (tam_post_proposal_id INT)
	INSERT INTO #relavent_tam_post_proposal_ids
		SELECT tam_post_proposal_id FROM dbo.udf_GetBestPostedAndAggregatedTamPostProposalIds(@year,@quarter,@month);

	CREATE TABLE #post_primary_demos (tam_post_id INT, primary_audience_id INT, guaranteed_audience_id INT, primary_audience_name VARCHAR(127), guaranteed_audience_name VARCHAR(127))
	INSERT INTO #post_primary_demos
		SELECT
			tpp.tam_post_id,
			tpdi.primary_audience_id,
			tpdi.guaranteed_audience_id,
			ap.name,
			ag.name
		FROM
			#relavent_tam_post_proposal_ids rtpp
			JOIN tam_post_proposals tpp ON tpp.id=rtpp.tam_post_proposal_id
			CROSS APPLY dbo.udf_GetTamPostDemoInfo(tpp.tam_post_id) tpdi
			JOIN audiences ap ON ap.id=tpdi.primary_audience_id
			JOIN audiences ag ON ag.id=tpdi.guaranteed_audience_id

	CREATE TABLE #contracted_networks (tam_post_id INT, is_msa BIT, tam_post_proposal_id INT, proposal_id INT, network_id INT, audience_id INT, delivery FLOAT, eq_delivery FLOAT, total_cost MONEY, rate MONEY, cpm MONEY, eq_cpm MONEY)
	-- primary demo contract details
	INSERT INTO #contracted_networks
		SELECT
			tp.id,
			tp.is_msa,
			tpp.id,
			tpp.posting_plan_proposal_id,
			pd.network_id,
			ppd.primary_audience_id,
			SUM(
				CAST(pd.num_spots AS FLOAT)
				*
				(pda.us_universe * pd.universal_scaling_factor * pda.rating)
			) 'delivery',
			SUM(
				CAST(pd.num_spots AS FLOAT)
				*
				((pda.us_universe * pd.universal_scaling_factor * pda.rating) * sl.delivery_multiplier)
			) 'eq_delivery',
			SUM(
				CAST(pd.num_spots AS MONEY)
				*
				pd.proposal_rate
			) 'total_cost',
			CASE 
				WHEN SUM(CAST(pd.num_spots AS MONEY)) > 0 THEN
					SUM(CAST(pd.num_spots AS MONEY) * pd.proposal_rate) / SUM(CAST(pd.num_spots AS MONEY))
				ELSE
					0
			END 'rate',
			NULL,
			NULL
		FROM
			#relavent_tam_post_proposal_ids rtpp
			JOIN tam_post_proposals tpp			(NOLOCK) ON tpp.id=rtpp.tam_post_proposal_id
			JOIN tam_posts tp					(NOLOCK) ON tp.id=tpp.tam_post_id
			LEFT JOIN #post_primary_demos ppd	(NOLOCK) ON ppd.tam_post_id=tp.id
			JOIN proposal_details pd			(NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
			JOIN proposal_detail_audiences pda	(NOLOCK) ON pda.proposal_detail_id=pd.id 
				AND pda.audience_id=ppd.primary_audience_id
			JOIN spot_lengths sl				(NOLOCK) ON sl.id=pd.spot_length_id
		GROUP BY
			tp.id,
			tp.is_msa,
			tpp.id,
			tpp.posting_plan_proposal_id,
			pd.network_id,
			ppd.primary_audience_id
	-- household contract details	
	INSERT INTO #contracted_networks
		SELECT
			tp.id,
			tp.is_msa,
			tpp.id,
			tpp.posting_plan_proposal_id,
			pd.network_id,
			31,
			SUM(
				CAST(pd.num_spots AS FLOAT)
				*
				(pda.us_universe * pd.universal_scaling_factor * pda.rating)
			) 'delivery',
			SUM(
				CAST(pd.num_spots AS FLOAT)
				*
				((pda.us_universe * pd.universal_scaling_factor * pda.rating) * sl.delivery_multiplier)
			) 'eq_delivery',
			SUM(
				CAST(pd.num_spots AS MONEY)
				*
				pd.proposal_rate
			) 'total_cost',
			CASE 
				WHEN SUM(CAST(pd.num_spots AS MONEY)) > 0 THEN
					SUM(CAST(pd.num_spots AS MONEY) * pd.proposal_rate) / SUM(CAST(pd.num_spots AS MONEY))
				ELSE
					0
			END 'rate',
			NULL,
			NULL
		FROM
			#relavent_tam_post_proposal_ids rtpp
			JOIN tam_post_proposals tpp			(NOLOCK) ON tpp.id=rtpp.tam_post_proposal_id
			JOIN tam_posts tp					(NOLOCK) ON tp.id=tpp.tam_post_id
			JOIN proposal_details pd			(NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
			JOIN proposal_detail_audiences pda	(NOLOCK) ON pda.proposal_detail_id=pd.id 
				AND pda.audience_id=31
			JOIN spot_lengths sl				(NOLOCK) ON sl.id=pd.spot_length_id
		GROUP BY
			tp.id,
			tp.is_msa,
			tpp.id,
			tpp.posting_plan_proposal_id,
			pd.network_id

	UPDATE #contracted_networks SET 
		cpm		= CASE WHEN delivery > 0	THEN total_cost / (delivery / 1000.0)		ELSE NULL END,
		eq_cpm  = CASE WHEN eq_delivery > 0 THEN total_cost / (eq_delivery / 1000.0)	ELSE NULL END


	CREATE TABLE #proposal_level_summary (tam_post_id INT, tam_post_proposal_id INT, post_source_code TINYINT, proposal_id INT, post_completed DATETIME, aggregation_completed DATETIME, media_month_id INT, demographic VARCHAR(31), audience_id INT, units FLOAT, rounded_units INT, subscribers BIGINT, hh_contracted_delivery FLOAT, demo_contracted_delivery FLOAT, contracted_total_cost MONEY, hh_delivery FLOAT, demo_delivery FLOAT, delivered_value MONEY, start_date DATETIME, end_date DATETIME, hh_delivery_index FLOAT, demo_delivery_index FLOAT, value_delivery_index FLOAT)
	INSERT INTO #proposal_level_summary
		SELECT
			tp.id,
			tpp.id,
			tpp.post_source_code,
			tpp.posting_plan_proposal_id,
			tpp.post_completed,
			tpp.aggregation_completed,
			p.posting_media_month_id,
			ppd.primary_audience_name,
			ppd.primary_audience_id,
			SUM(tpnd.units) 'dr_units',
			SUM(tpnd.rounded_units) 'dr_rounded_units',
			SUM(tpnd.subscribers) 'subscribers',
			SUM(CASE tp.is_equivalized WHEN 0 THEN c_hh.delivery WHEN 1 THEN c_hh.eq_delivery END) 'hh_contracted_delivery',
			SUM(CASE tp.is_equivalized WHEN 0 THEN c_dm.delivery WHEN 1 THEN c_dm.eq_delivery END) 'demo_contracted_delivery',
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
			SUM(tpnd.units * c_hh.rate) 'delivered_value',
			--CASE tp.rate_card_type_id 
			--	WHEN 1 THEN SUM(
			--		(CASE tp.is_equivalized
			--			WHEN 0 THEN
			--				tpnda_dm.delivery
			--			WHEN 1 THEN
			--				tpnda_dm.eq_delivery
			--		END / 1000.0)
			--		*
			--		CASE tp.is_equivalized
			--			WHEN 0 THEN
			--				c_dm.cpm
			--			WHEN 1 THEN
			--				c_dm.eq_cpm
			--		END
			--	) 
			--	WHEN 2 THEN SUM(tpnd.units * c_hh.rate) 
			--END 'delivered_value',
			p.start_date,
			p.end_date,
			NULL,
			NULL,
			NULL
		FROM
			#relavent_tam_post_proposal_ids rtpp
			JOIN tam_post_proposals tpp	(NOLOCK) ON tpp.id=rtpp.tam_post_proposal_id
			JOIN tam_posts tp ON tp.id=tpp.tam_post_id
			LEFT JOIN #post_primary_demos ppd ON ppd.tam_post_id=tp.id
			LEFT JOIN #contracted_networks c_hh ON c_hh.tam_post_proposal_id=tpp.id AND c_hh.audience_id=31
			LEFT JOIN #contracted_networks c_dm ON c_dm.tam_post_proposal_id=tpp.id AND c_dm.network_id=c_hh.network_id AND c_dm.audience_id=ppd.primary_audience_id
			LEFT JOIN tam_post_network_details tpnd ON tpnd.tam_post_proposal_id=tpp.id AND tpnd.network_id=c_hh.network_id AND tpnd.[enabled]=1
			LEFT JOIN tam_post_network_detail_audiences tpnda_hh ON tpnda_hh.tam_post_network_detail_id=tpnd.id AND tpnda_hh.audience_id=31
			LEFT JOIN tam_post_network_detail_audiences tpnda_dm ON tpnda_dm.tam_post_network_detail_id=tpnd.id AND tpnda_dm.audience_id=ppd.primary_audience_id
			JOIN proposals p ON p.id=tpp.posting_plan_proposal_id
		GROUP BY
			tp.id,
			tpp.id,
			tpp.post_source_code,
			tpp.posting_plan_proposal_id,
			tpp.post_completed,
			tpp.aggregation_completed,
			p.posting_media_month_id,
			ppd.primary_audience_name,
			ppd.primary_audience_id,
			tp.rate_card_type_id,
			tp.is_equivalized,
			p.start_date,
			p.end_date
		ORDER BY
			tp.id,
			tpp.id
			
	UPDATE #proposal_level_summary SET
		hh_delivery_index = 
			CASE
				WHEN hh_contracted_delivery > 0 THEN
					hh_delivery / hh_contracted_delivery
				ELSE
					NULL
			END,
		demo_delivery_index = 
			CASE
				WHEN demo_contracted_delivery > 0 THEN
					demo_delivery / demo_contracted_delivery
				ELSE
					NULL
			END,
		value_delivery_index = 
			CASE
				WHEN contracted_total_cost > 0 THEN
					delivered_value / contracted_total_cost
				ELSE
					NULL
			END
	
	-- return data set
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
	WHERE
		dp.id IN (
			SELECT tam_post_id FROM #proposal_level_summary
		)
	ORDER BY
		dp.post_setup_advertiser,
		dp.post_setup_product
	SELECT * FROM #proposal_level_summary	ORDER BY tam_post_id


	DROP TABLE #post_primary_demos;
	DROP TABLE #relavent_tam_post_proposal_ids;
	DROP TABLE #contracted_networks;
	DROP TABLE #proposal_level_summary;
END
