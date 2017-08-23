-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/9/2012
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_ExportDealAnalysisReport 25386, 1, 2010
CREATE PROCEDURE [dbo].[usp_PCS_ExportDealAnalysisReport]
	@proposal_id INT,
	@quarter INT,
	@year INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @audience_id INT;
	DECLARE @network_rate_card_id INT;
	DECLARE @quarter_start_date DATETIME;
	DECLARE @quarter_end_date DATETIME;

	SET @quarter_start_date = (SELECT MIN(start_date) FROM media_months mm (NOLOCK) WHERE CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter AND year=@year)
	SET @quarter_end_date = (SELECT MAX(end_date) FROM media_months mm (NOLOCK) WHERE CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter AND year=@year)
	SET @audience_id = (SELECT audience_id FROM proposal_audiences (NOLOCK) WHERE proposal_id=@proposal_id AND ordinal=1)
	SET @network_rate_card_id = (SELECT network_rate_card_id FROM proposals (NOLOCK) WHERE id=@proposal_id)

	CREATE TABLE #hh_all_deals (network_id INT, daypart_id INT, all_deals_gross_cost MONEY, all_deals_eq_hh_delivery FLOAT, all_deals_eq_hh_cpm MONEY)
	INSERT INTO #hh_all_deals
		SELECT
			n.id,
			d.id,
			SUM(pd.proposal_rate * pd.num_spots),
			SUM(((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier * pd.num_spots),
			CAST(SUM(pd.proposal_rate * pd.num_spots) / SUM(((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier * pd.num_spots) AS MONEY)
		FROM
			proposal_detail_audiences pda (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
			JOIN networks n (NOLOCK) ON n.id=pd.network_id
			JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
			JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			p.proposal_status_id=4
			AND p.advertiser_company_id<>39680
			AND pda.audience_id=31
			AND (pda.us_universe > 0 AND pd.universal_scaling_factor > 0 AND pda.rating > 0 AND sl.delivery_multiplier > 0)
			AND p.network_rate_card_id=@network_rate_card_id
			AND pd.num_spots>0
			AND pd.proposal_id<>@proposal_id
			AND (p.start_date <= @quarter_end_date AND p.end_date >= @quarter_start_date)
		GROUP BY
			n.id,
			d.id

	CREATE TABLE #demo_all_deals (network_id INT, daypart_id INT, all_deals_gross_cost MONEY, all_deals_eq_demo_delivery FLOAT, all_deals_eq_demo_cpm MONEY)
	INSERT INTO #demo_all_deals
		SELECT
			pd.network_id,
			d.id,
			SUM(pd.proposal_rate * pd.num_spots),
			SUM(((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier * pd.num_spots),
			CAST(SUM(pd.proposal_rate * pd.num_spots) / SUM(((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier * pd.num_spots) AS MONEY)
		FROM
			proposal_detail_audiences pda (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
			JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
			JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			p.proposal_status_id=4
			AND p.advertiser_company_id<>39680
			AND pda.audience_id=@audience_id
			AND (pda.us_universe > 0 AND pd.universal_scaling_factor > 0 AND pda.rating > 0 AND sl.delivery_multiplier > 0)
			AND p.network_rate_card_id=@network_rate_card_id
			AND pd.num_spots>0
			AND pd.proposal_id<>@proposal_id
			AND (p.start_date <= @quarter_end_date AND p.end_date >= @quarter_start_date)
		GROUP BY
			pd.network_id,
			d.id

	SELECT
		p.print_title,
		p.name,
		'CPM Comparisons to all Clients and Rate Card [' + nrcb.name + '] for ' + CASE @quarter WHEN 1 THEN '1st' WHEN 2 THEN '2nd' WHEN 3 THEN '3rd' WHEN 4 THEN '4th' END + ' Qtr, ' + CAST(@year AS VARCHAR(4)) [subtitle],
		dbo.GetAudienceStringFromID(pa.audience_id) [demo],
		dbo.GetProposalVersionIdentifier(@proposal_id) [version_identifier],
		pa.audience_id,
		nrcb.base_ratings_media_month_id,
		nrcb.base_coverage_universe_media_month_id,
		(SELECT TOP 1 sales_model_id FROM proposal_sales_models psm (NOLOCK) WHERE psm.proposal_id=p.id) [sales_model_id],
		dbo.GetNetworkRateCardBookStartDate(nrcb.id) [rate_card_start_date],
		dbo.GetNetworkRateCardBookEndDate(nrcb.id) [rate_card_end_date]
	FROM
		proposals p (NOLOCK)
		JOIN network_rate_cards nrc (NOLOCK) ON nrc.id=p.network_rate_card_id
		JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
		LEFT JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=p.id AND ordinal=1
	WHERE
		p.id=@proposal_id

	SELECT
		n.network_id,
		n.code,
		d.daypart_text,
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
		d.sun,
		dbo.GetProposalDetailSubTotalCost(pd.id) 'deal_gross_cost',
		dbo.GetProposalDetailCPMEquivalized(pd.id,31) 'deal_eq_hh_cpm',
		dbo.GetProposalDetailCPMEquivalized(pd.id,@audience_id) 'deal_eq_demo_cpm',
		dbo.GetProposalDetailDeliveryEquivalized(pd.id,31) * CAST(pd.num_spots AS FLOAT) 'deal_eq_hh_delivery',
		dbo.GetProposalDetailDeliveryEquivalized(pd.id,@audience_id) * CAST(pd.num_spots AS FLOAT) 'deal_eq_demo_delivery',
		nrcd.hh_cpm 'rate_card_hh_cpm',
		nrcd.hh_delivery / 1000.0 'rate_card_hh_delivery',
		nrcr.rate 'rate_card_rate',
		#hh_all_deals.all_deals_gross_cost,
		#hh_all_deals.all_deals_eq_hh_delivery,
		#hh_all_deals.all_deals_eq_hh_cpm,
		#demo_all_deals.all_deals_eq_demo_delivery,
		#demo_all_deals.all_deals_eq_demo_cpm,
		nrcd.hh_coverage_universe / nrcd.hh_us_universe 'rate_card_universal_scaling_factor'
	FROM
		proposal_details pd (NOLOCK)
		JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
		JOIN uvw_network_universe n	(NOLOCK) ON n.network_id=pd.network_id AND (n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL))
		JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
		JOIN network_rate_card_details nrcd (NOLOCK) ON nrcd.network_rate_card_id=p.network_rate_card_id AND nrcd.network_id=pd.network_id
		JOIN network_rate_card_rates nrcr (NOLOCK) ON nrcr.network_rate_card_detail_id=nrcd.id AND nrcr.spot_length_id=1
		LEFT JOIN #hh_all_deals ON #hh_all_deals.network_id=n.network_id AND #hh_all_deals.daypart_id=d.id
		LEFT JOIN #demo_all_deals ON #demo_all_deals.network_id=n.network_id AND #demo_all_deals.daypart_id=d.id
	WHERE
		pd.proposal_id=@proposal_id
		AND pd.num_spots>0
	ORDER BY
		n.code,
		d.daypart_text

	DROP TABLE #hh_all_deals;
	DROP TABLE #demo_all_deals;
END
