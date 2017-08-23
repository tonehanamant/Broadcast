-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/11/2016
-- Description:	
-- =============================================
-- usp_PCS_GetSalesExplorerDto 58362
CREATE PROCEDURE [dbo].[usp_PCS_GetSalesExplorerDto] 
	@proposal_id INT
AS
BEGIN
	-- header
	SELECT
		dp.id,
		dp.advertiser,
		dp.agency,
		dp.product,
		dp.title,
		dp.salesperson,
		dp.start_date,
		dp.end_date,
		dp.flight_text,
		a.code,
		a.id,
		p.budget,
		rtc.name 'type'
	FROM
		uvw_display_proposals dp
		JOIN proposals p ON p.id=dp.id
		JOIN proposal_audiences pa ON pa.proposal_id=p.id
			AND pa.ordinal=p.guarantee_type
		JOIN audiences a ON a.id=pa.audience_id
		JOIN rate_card_types rtc ON rtc.id=p.rate_card_type_id
	WHERE
		dp.id=@proposal_id

	-- details
	SELECT
		pd.id,
		pd.network_id,
		n.code,
		sl.length,
		d.id,d.start_time,d.end_time,
		d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun,
		pd.start_date,
		pd.end_date,
		pd.num_spots,
		dbo.GetProposalDetailDeliveryEquivalized(pd.id,31) * 1000.0 'hh_eq_imp_per_spot',
		dbo.GetProposalDetailDeliveryEquivalized(pd.id,pa.audience_id) * 1000.0 'gd_eq_imp_per_spot',
		dbo.GetProposalDetailTotalDeliverySpecifyEq(pd.id,31,1) * 1000.0 'hh_eq_imp_total',
		dbo.GetProposalDetailTotalDeliverySpecifyEq(pd.id,pa.audience_id,1) * 1000.0 'gd_eq_imp_total',
		CAST(pd.num_spots * pd.topography_universe AS BIGINT) 'contracted_subscribers',
		pd.topography_universe 'hh_coverage_universe',
		pda_gd.us_universe * pd.universal_scaling_factor 'gd_coverage_universe',
		pda_hh.rating,
		pda_gd.rating 'gd_rating',
		dbo.GetProposalDetailCPMEquivalized(pd.id,31) 'hh_eq_cpm',
		dbo.GetProposalDetailCPMEquivalized(pd.id,pa.audience_id) 'gd_eq_cpm',
		dbo.GetProposalDetailVPVH(pd.id,pa.audience_id) 'vpvh',
		pda_hh.us_universe,
		pda_gd.us_universe,
		pd.proposal_rate 'rate',
		pd.proposal_rate * pd.num_spots 'total_cost',
		dbo.GetProposalDetailGrp(pd.id) 'grp',
		dbo.GetProposalDetailAudienceNEQRating(pd.id,pa.audience_id) 'trp'
	FROM
		proposal_details pd
		JOIN proposals p ON p.id=pd.proposal_id
		JOIN uvw_network_universe n ON n.network_id=pd.network_id
			AND n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL)
		JOIN spot_lengths sl ON sl.id=pd.spot_length_id
		JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
		JOIN proposal_audiences pa ON pa.proposal_id=p.id
			AND pa.ordinal=p.guarantee_type
		JOIN proposal_detail_audiences pda_hh ON pda_hh.proposal_detail_id=pd.id
			AND pda_hh.audience_id=31
		JOIN proposal_detail_audiences pda_gd ON pda_gd.proposal_detail_id=pd.id
			AND pda_gd.audience_id=pa.audience_id
	WHERE
		pd.proposal_id=@proposal_id
	ORDER BY
		n.code,
		pd.id

	-- weekly details
	SELECT
		pdw.proposal_detail_id,
		mw.media_month_id,
		pdw.media_week_id,
		mm.media_month,
		mw.week_number,
		mw.start_date,
		mw.end_date,
		pdw.units
	FROM
		proposal_detail_worksheets pdw
		JOIN proposal_details pd ON pd.id=pdw.proposal_detail_id
		JOIN media_weeks mw ON mw.id=pdw.media_week_id
		JOIN media_months mm ON mm.id=mw.media_month_id
	WHERE
		pd.proposal_id=@proposal_id
	ORDER BY
		pdw.proposal_detail_id,
		mw.start_date

	-- component inventory dayparts
	SELECT
		d.id,d.code,d.name,d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun
	FROM
		vw_ccc_daypart d
	WHERE
		--M-SU 3 hour increments
		(d.start_time=0 AND d.end_time=10799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
		OR (d.start_time=10800 AND d.end_time=21599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
		OR (d.start_time=21600 AND d.end_time=32399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
		OR (d.start_time=32400 AND d.end_time=43199 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
		OR (d.start_time=43200 AND d.end_time=53999 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
		OR (d.start_time=54000 AND d.end_time=64799 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
		OR (d.start_time=64800 AND d.end_time=75599 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
		OR (d.start_time=75600 AND d.end_time=86399 AND d.mon=1 AND d.tue=1 AND d.wed=1 AND d.thu=1 AND d.fri=1 AND d.sat=1 AND d.sun=1)
	ORDER BY
		d.start_time
END