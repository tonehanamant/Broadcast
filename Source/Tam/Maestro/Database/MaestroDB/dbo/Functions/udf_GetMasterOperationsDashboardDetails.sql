-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/13/2014
-- Updated:		3/16/2015 to include fast track gap percentages.
-- Description:	
-- =============================================
-- SELECT * FROM tam_post_proposals WHERE tam_post_id=1002467
-- SELECT * FROM proposal_audiences WHERE proposal_id=51081
-- SELECT * FROM dbo.udf_GetMasterOperationsDashboardDetails(1003670,60075,NULL)
CREATE FUNCTION [dbo].[udf_GetMasterOperationsDashboardDetails]
(
	@tam_post_id INT,
	@proposal_id INT,
	@audience_id INT
)
RETURNS @return TABLE
(
	source VARCHAR(63), 
	tam_post_id INT, 
	tam_post_proposal_id INT, 
	proposal_id INT, 
	units FLOAT, 
	rounded_units INT, 
	subscribers BIGINT, 
	hh_contracted_delivery FLOAT, 
	demo_contracted_delivery FLOAT, 
	contracted_total_cost MONEY, 
	hh_delivery FLOAT, 
	demo_delivery FLOAT, 
	delivered_value MONEY
) 
AS
BEGIN
	DECLARE @media_month_id INT;
	DECLARE @tam_post_proposal_id INT;
	DECLARE @fast_track_tam_post_proposal_id INT;
	DECLARE @msa_tam_post_proposal_id INT;
	DECLARE @is_msa_post_available BIT;
	DECLARE @is_affidavit_post_available BIT;
	DECLARE @is_affidavit_fast_track_available BIT;
	DECLARE @is_post_log_available BIT;

	-- meida month of posting plan
	SELECT
		@media_month_id = p.posting_media_month_id
	FROM
		dbo.proposals p (NOLOCK)
	WHERE
		p.id=@proposal_id;
		
	-- tam_post_proposal_id's for post/proposal
	SELECT
		@tam_post_proposal_id = tpp.id
	FROM
		dbo.tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id=@proposal_id
		AND tpp.post_source_code=0;
	
	SELECT
		@fast_track_tam_post_proposal_id = tpp.id
	FROM
		dbo.tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id=@proposal_id
		AND tpp.post_source_code=1;
		
	SELECT
		@msa_tam_post_proposal_id = tpp.id
	FROM
		dbo.tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND tpp.posting_plan_proposal_id=@proposal_id
		AND tpp.post_source_code=2;
				

	-- is msa post available (verify it's been run and that we have data)
	SELECT
		@is_msa_post_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	FROM
		dbo.tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.id=@msa_tam_post_proposal_id
		AND tpp.post_completed IS NOT NULL
		AND tpp.aggregation_completed IS NOT NULL
		AND tpp.aggregation_status_code=1;
	
	-- double check we actually have data if MSA post
	IF @is_msa_post_available = 1 
	BEGIN
		SELECT 
			@is_msa_post_available = 
				CASE 
					WHEN 
						CASE tp.rate_card_type_id WHEN 1 THEN SUM(tpnda.delivery) ELSE SUM(tpnda.dr_delivery) END > 0 
					THEN 
						1 
					ELSE 
						0 
				END
		FROM 
			tam_post_network_details tpnd (NOLOCK)
			JOIN tam_post_network_detail_audiences tpnda (NOLOCK) ON tpnda.tam_post_network_detail_id=tpnd.id
				AND tpnda.audience_id=31
			JOIN tam_post_proposals tpp (NOLOCK)ON tpp.id=tpnd.tam_post_proposal_id
			JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
		WHERE
			tpp.id=@msa_tam_post_proposal_id
		GROUP BY
			tp.rate_card_type_id
	END

	-- is affidavit post available
	IF @is_msa_post_available = 0
	BEGIN
		SELECT
			@is_affidavit_post_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
		FROM
			dbo.tam_post_proposals tpp (NOLOCK)
		WHERE
			tpp.id=@tam_post_proposal_id
			AND tpp.post_completed IS NOT NULL
			AND tpp.aggregation_completed IS NOT NULL
			AND tpp.aggregation_status_code=1;
	END	
	
	-- is affidavit fast track available
	IF @is_msa_post_available = 0 AND @is_affidavit_post_available = 0
	BEGIN
		SELECT
			@is_affidavit_fast_track_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
		FROM
			dbo.tam_post_proposals tpp (NOLOCK)
		WHERE
			tpp.id=@fast_track_tam_post_proposal_id
			AND tpp.post_completed IS NOT NULL
			AND tpp.aggregation_completed IS NOT NULL
			AND tpp.aggregation_status_code=1;
	END

	-- is post log data available	
	IF @is_msa_post_available = 0 AND @is_affidavit_post_available = 0 AND @is_affidavit_fast_track_available = 0
	BEGIN
		SELECT
			@is_post_log_available = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
		FROM
			postlog_staging.mart.weekly_report wr (NOLOCK)
		WHERE
			wr.media_month_id=@media_month_id
			AND wr.tam_post_proposal_id=@tam_post_proposal_id;
	END


	IF @is_msa_post_available = 1
	BEGIN
		INSERT INTO @return
			SELECT
				'MSA' 'source',
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
								ISNULL(tpnda_dm.delivery, tpnda_hh.delivery)
							WHEN 1 THEN
								ISNULL(tpnda_dm.eq_delivery, tpnda_hh.eq_delivery)
						END / 1000.0)
						*
						CASE tp.is_equivalized
							WHEN 0 THEN
								ISNULL(c_dm.cpm, c_hh.cpm)
							WHEN 1 THEN
								ISNULL(c_dm.eq_cpm, c_hh.eq_cpm)
						END
					) 
					WHEN 2 THEN SUM(tpnd.rounded_units * c_hh.rate) 
				END 'delivered_value'
			FROM
				dbo.tam_posts tp (NOLOCK)
				JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
					AND tpp.id=@msa_tam_post_proposal_id
				CROSS APPLY dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31) c_hh
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @audience_id) c_dm ON c_dm.network_id=c_hh.network_id
				LEFT JOIN dbo.tam_post_network_details tpnd (NOLOCK) ON tpnd.tam_post_proposal_id=tpp.id AND tpnd.network_id=c_hh.network_id AND tpnd.[enabled]=1
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_hh (NOLOCK) ON tpnda_hh.tam_post_network_detail_id=tpnd.id AND tpnda_hh.audience_id=31
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_dm (NOLOCK) ON tpnda_dm.tam_post_network_detail_id=tpnd.id AND tpnda_dm.audience_id=@audience_id
			GROUP BY
				tp.is_equivalized,
				tp.rate_card_type_id,
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id;
	END
	ELSE 
	IF @is_affidavit_post_available = 1
	BEGIN
		INSERT INTO @return
			SELECT
				'AFF-PT' 'source',
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
								ISNULL(tpnda_dm.delivery, tpnda_hh.delivery)
							WHEN 1 THEN
								ISNULL(tpnda_dm.eq_delivery, tpnda_hh.eq_delivery)
						END / 1000.0)
						*
						CASE tp.is_equivalized
							WHEN 0 THEN
								ISNULL(c_dm.cpm, c_hh.cpm)
							WHEN 1 THEN
								ISNULL(c_dm.eq_cpm, c_hh.eq_cpm)
						END
					) 
					WHEN 2 THEN SUM(tpnd.rounded_units * c_hh.rate) 
				END 'delivered_value'
			FROM
				dbo.tam_posts tp (NOLOCK)
				JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
					AND tpp.id=@tam_post_proposal_id
				CROSS APPLY dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31) c_hh
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @audience_id) c_dm ON c_dm.network_id=c_hh.network_id
				LEFT JOIN dbo.tam_post_network_details tpnd (NOLOCK) ON tpnd.tam_post_proposal_id=tpp.id AND tpnd.network_id=c_hh.network_id AND tpnd.[enabled]=1
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_hh (NOLOCK) ON tpnda_hh.tam_post_network_detail_id=tpnd.id AND tpnda_hh.audience_id=31
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_dm (NOLOCK) ON tpnda_dm.tam_post_network_detail_id=tpnd.id AND tpnda_dm.audience_id=@audience_id
			GROUP BY
				tp.is_equivalized,
				tp.rate_card_type_id,
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id;
	END
	ELSE IF @is_affidavit_fast_track_available = 1
	BEGIN			
		INSERT INTO @return
			SELECT
				'AFF-FT' 'source',
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
				--CASE tp.is_equivalized
				--	WHEN 0 THEN
				--		CASE tp.rate_card_type_id 
				--			WHEN 1 THEN SUM(tpnda_hh.delivery) 
				--			WHEN 2 THEN SUM(tpnda_hh.dr_delivery) 
				--		END
				--	WHEN 1 THEN
				--		CASE tp.rate_card_type_id 
				--			WHEN 1 THEN SUM(tpnda_hh.eq_delivery) 
				--			WHEN 2 THEN SUM(tpnda_hh.dr_eq_delivery) 
				--		END
				--END 'hh_delivery',
				-- contracted * ((delivered / contracted) + gap projection percentage)
			
				CASE 
					WHEN CASE tp.is_equivalized WHEN 0 THEN SUM(c_hh.delivery) WHEN 1 THEN SUM(c_hh.eq_delivery) END > 0 THEN 
						CASE tp.is_equivalized
							WHEN 0 THEN
								SUM(c_hh.delivery)
							WHEN 1 THEN
								SUM(c_hh.eq_delivery)
						END
						*
						(
							(CASE tp.is_equivalized
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
							END
							/
							CASE tp.is_equivalized
								WHEN 0 THEN
									SUM(c_hh.delivery)
								WHEN 1 THEN
									SUM(c_hh.eq_delivery)
							END) + CASE WHEN tpgp.gap_projection IS NOT NULL THEN tpgp.gap_projection / 100.0 WHEN dftgp.gap_projection IS NOT NULL THEN dftgp.gap_projection / 100.0 ELSE 0 END
						)
					ELSE
						0
				END 'hh_delivery',
			
				--CASE tp.is_equivalized
				--	WHEN 0 THEN
				--		CASE tp.rate_card_type_id 
				--			WHEN 1 THEN SUM(tpnda_dm.delivery) 
				--			WHEN 2 THEN SUM(tpnda_dm.dr_delivery) 
				--		END
				--	WHEN 1 THEN
				--		CASE tp.rate_card_type_id 
				--			WHEN 1 THEN SUM(tpnda_dm.eq_delivery) 
				--			WHEN 2 THEN SUM(tpnda_dm.dr_eq_delivery) 
				--		END
				--END 'demo_delivery',
			
				CASE 
					WHEN CASE tp.is_equivalized WHEN 0 THEN SUM(c_dm.delivery) WHEN 1 THEN SUM(c_dm.eq_delivery) END > 0 THEN 
						CASE tp.is_equivalized
							WHEN 0 THEN
								SUM(c_dm.delivery)
							WHEN 1 THEN
								SUM(c_dm.eq_delivery)
						END
						*
						(
							(CASE tp.is_equivalized
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
							END
							/
							CASE tp.is_equivalized
								WHEN 0 THEN
									SUM(c_dm.delivery)
								WHEN 1 THEN
									SUM(c_dm.eq_delivery)
							END) + CASE WHEN tpgp.gap_projection IS NOT NULL THEN tpgp.gap_projection / 100.0 WHEN dftgp.gap_projection IS NOT NULL THEN dftgp.gap_projection / 100.0 ELSE 0 END
						)
					ELSE
						0
				END 'demo_delivery',
			
				CASE 
					WHEN SUM(c_hh.total_cost) > 0 THEN 
						SUM(c_hh.total_cost)
						*
						(
							(CASE tp.rate_card_type_id 
								WHEN 1 THEN SUM(
									(CASE tp.is_equivalized
										WHEN 0 THEN
											ISNULL(tpnda_dm.delivery, tpnda_hh.delivery)
										WHEN 1 THEN
											ISNULL(tpnda_dm.eq_delivery, tpnda_hh.eq_delivery)
									END / 1000.0)
									*
									CASE tp.is_equivalized
										WHEN 0 THEN
											ISNULL(c_dm.cpm, c_hh.cpm)
										WHEN 1 THEN
											ISNULL(c_dm.eq_cpm, c_hh.eq_cpm)
									END
								) 
								WHEN 2 THEN SUM(tpnd.rounded_units * c_hh.rate) 
							END
							/
							SUM(c_hh.total_cost)) + CASE WHEN tpgp.gap_projection IS NOT NULL THEN tpgp.gap_projection / 100.0 WHEN dftgp.gap_projection IS NOT NULL THEN dftgp.gap_projection / 100.0 ELSE 0 END
						)
					ELSE
						0
				END 'delivered_value'
			FROM
				dbo.tam_posts tp (NOLOCK)
				JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
					AND tpp.id=@fast_track_tam_post_proposal_id
				CROSS APPLY dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31) c_hh
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @audience_id) c_dm ON c_dm.network_id=c_hh.network_id
				LEFT JOIN dbo.tam_post_network_details tpnd (NOLOCK) ON tpnd.tam_post_proposal_id=tpp.id AND tpnd.network_id=c_hh.network_id AND tpnd.[enabled]=1
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_hh (NOLOCK) ON tpnda_hh.tam_post_network_detail_id=tpnd.id AND tpnda_hh.audience_id=31
				LEFT JOIN dbo.tam_post_network_detail_audiences tpnda_dm (NOLOCK) ON tpnda_dm.tam_post_network_detail_id=tpnd.id AND tpnda_dm.audience_id=@audience_id
				LEFT JOIN dbo.tam_post_gap_projections tpgp (NOLOCK) ON tpgp.media_month_id=@media_month_id AND tpgp.tam_post_id=tp.id AND tpgp.rate_card_type_id=tp.rate_card_type_id
				LEFT JOIN dbo.default_fast_track_gap_projections dftgp (NOLOCK) ON dftgp.media_month_id=@media_month_id AND dftgp.rate_card_type_id=tp.rate_card_type_id
		GROUP BY
				tp.is_equivalized,
				tp.rate_card_type_id,
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id,
				CASE WHEN tpgp.gap_projection IS NOT NULL THEN tpgp.gap_projection / 100.0 WHEN dftgp.gap_projection IS NOT NULL THEN dftgp.gap_projection / 100.0 ELSE 0 END;
	END
	ELSE IF @is_post_log_available = 1
	BEGIN		
		INSERT INTO @return
		SELECT
			'PL-ACT' 'source',
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
			SUM(da.actual_hh_impressions) 'hh_delivery',
			SUM(da.actual_demo_impressions) 'demo_delivery',
			SUM(actual_delivered_value) 'delivered_value'
		FROM
			dbo.tam_posts tp (NOLOCK)
			JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
				AND tpp.id=@tam_post_proposal_id
			CROSS APPLY dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31) c_hh
			LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @audience_id) c_dm ON c_dm.network_id=c_hh.network_id
			LEFT JOIN (
				SELECT
					pd.network_id,
					SUM(wr.subscribers) 'actual_subscribers',
					SUM(wr.hh_impressions) 'actual_hh_impressions',
					SUM(wr.delivered_value) 'actual_delivered_value',
					SUM(CASE WHEN @audience_id=wr.guaranteed_audience_id THEN wr.guaranteed_impressions ELSE 0 END) 'actual_demo_impressions'
				FROM
					postlog_staging.mart.weekly_report wr (NOLOCK) 
					JOIN proposal_details pd (NOLOCK) ON pd.id=wr.proposal_detail_id
				WHERE
					wr.media_month_id=@media_month_id
					AND wr.tam_post_proposal_id=@tam_post_proposal_id
				GROUP BY
					pd.network_id
			) da ON da.network_id=c_hh.network_id
			JOIN dbo.proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		GROUP BY
			tp.id,
			tpp.id,
			tpp.post_source_code,
			tpp.posting_plan_proposal_id,
			tp.is_equivalized,
			tp.rate_card_type_id
		ORDER BY
			tp.id,
			tpp.id;	
	END
	ELSE
	BEGIN			
		INSERT INTO @return
			SELECT
				'EST' 'source',
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id,
				SUM(c_hh.num_spots) 'dr_units',
				SUM(c_hh.num_spots) 'dr_rounded_units',
				SUM(c_hh.num_spots * c_hh.universe) 'subscribers',
				CASE tp.is_equivalized
					WHEN 0 THEN SUM(c_hh.delivery)
					WHEN 1 THEN SUM(c_hh.eq_delivery)
				END 'contracted_hh_delivery',
				CASE tp.is_equivalized
					WHEN 0 THEN SUM(c_dm.delivery)
					WHEN 1 THEN SUM(c_dm.eq_delivery)
				END 'contracted_demo_delivery',
				SUM(c_hh.total_cost) 'contracted_total_cost',
				CASE tp.is_equivalized
					WHEN 0 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(c_hh.delivery) 
							WHEN 2 THEN SUM(c_hh.num_spots * c_hh.delivery) 
						END
					WHEN 1 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(c_hh.eq_delivery) 
							WHEN 2 THEN SUM(c_hh.num_spots * c_hh.eq_delivery) 
						END
				END 'hh_delivery',
				CASE tp.is_equivalized
					WHEN 0 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(c_dm.delivery) 
							WHEN 2 THEN SUM(c_dm.num_spots * c_dm.delivery) 
						END
					WHEN 1 THEN
						CASE tp.rate_card_type_id 
							WHEN 1 THEN SUM(c_dm.eq_delivery) 
							WHEN 2 THEN SUM(c_dm.num_spots * c_dm.eq_delivery) 
						END
				END 'demo_delivery',
				CASE tp.rate_card_type_id 
					WHEN 1 THEN SUM(
						(CASE tp.is_equivalized
							WHEN 0 THEN c_dm.delivery
							WHEN 1 THEN c_dm.eq_delivery
						END / 1000.0)
						*
						CASE tp.is_equivalized
							WHEN 0 THEN
								ISNULL(c_dm.cpm, c_hh.cpm)
							WHEN 1 THEN
								ISNULL(c_dm.eq_cpm, c_hh.eq_cpm)
						END
					) 
					WHEN 2 THEN SUM(c_hh.num_spots * c_hh.rate) 
				END 'delivered_value'
			FROM
				dbo.tam_posts tp (NOLOCK)
				JOIN dbo.tam_post_proposals tpp (NOLOCK) ON tpp.tam_post_id=tp.id
					AND tpp.id=@tam_post_proposal_id
				CROSS APPLY dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, 31) c_hh
				LEFT JOIN dbo.udf_GetProposalNetworkContractDataSet(@proposal_id, @audience_id) c_dm ON c_dm.network_id=c_hh.network_id
			GROUP BY
				tp.is_equivalized,
				tp.rate_card_type_id,
				tp.id,
				tpp.id,
				tpp.posting_plan_proposal_id;
	END

	UPDATE
		@return
	SET
		delivered_value=ISNULL(delivered_value, 0),
		hh_delivery=ISNULL(hh_delivery, 0),
		demo_delivery=ISNULL(demo_delivery, 0),
		units=ISNULL(units, 0),
		rounded_units=ISNULL(rounded_units, 0),
		subscribers=ISNULL(subscribers, 0)

	RETURN;
END
