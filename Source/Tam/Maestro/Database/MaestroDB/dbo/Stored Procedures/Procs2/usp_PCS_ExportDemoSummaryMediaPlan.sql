CREATE PROCEDURE [dbo].[usp_PCS_ExportDemoSummaryMediaPlan]
	@proposal_id int
AS
BEGIN
	SET NOCOUNT ON;

	-- proposal
	SELECT 
		products.name,
		proposals.advertiser_company_id,
		proposals.agency_company_id,
		proposals.name,
		spot_lengths.length,
		proposals.flight_text,
		proposals.date_created,
		proposals.version_number,
		dbo.GetProposalRatePercentage(@proposal_id) 'percentage', 
		proposals.proposal_status_id,
		dbo.GetProposalTotalCost(@proposal_id) 'total_gross_cost',
		dbo.GetProposalAudienceNEQRating(@proposal_id,31) 'total_hh_rating',
		dbo.GetProposalTotalUnits(@proposal_id) 'total_units',
		proposals.rate_card_type_id,
		proposals.is_equivalized,
		proposals.rating_source_id,
		proposals.buyer_note,
		proposals.print_title,
		dbo.GetProposalSalesModel(proposals.id) 'sales_model_id'
	FROM 
		proposals (NOLOCK)
		LEFT JOIN products (NOLOCK) ON products.id=proposals.product_id 
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=proposals.default_spot_length_id 
	WHERE 
		proposals.id=@proposal_id

	-- daypart
	SELECT
		TOP 1
		vw_ccc_daypart.id,
		vw_ccc_daypart.code,
		vw_ccc_daypart.name,
		vw_ccc_daypart.start_time,
		vw_ccc_daypart.end_time,
		vw_ccc_daypart.mon,
		vw_ccc_daypart.tue,
		vw_ccc_daypart.wed,
		vw_ccc_daypart.thu,
		vw_ccc_daypart.fri,
		vw_ccc_daypart.sat,
		vw_ccc_daypart.sun,
		COUNT(*)
	FROM 
		proposal_details (NOLOCK) 
		JOIN vw_ccc_daypart (NOLOCK) ON vw_ccc_daypart.id=proposal_details.daypart_id
	WHERE 
		proposal_id=@proposal_id
	GROUP BY 
		vw_ccc_daypart.id,
		vw_ccc_daypart.code,
		vw_ccc_daypart.name,
		vw_ccc_daypart.start_time,
		vw_ccc_daypart.end_time,
		vw_ccc_daypart.mon,
		vw_ccc_daypart.tue,
		vw_ccc_daypart.wed,
		vw_ccc_daypart.thu,
		vw_ccc_daypart.fri,
		vw_ccc_daypart.sat,
		vw_ccc_daypart.sun
	ORDER BY
		COUNT(*) DESC

	-- audiences
	SELECT 
		pa.audience_id,
		a.code 'audience_code',
		a.name 'audience_name',
		dbo.GetProposalAudienceNEQRating(pa.proposal_id,pa.audience_id) 'neq_rating'
	FROM 
		proposal_audiences pa (NOLOCK) 
		JOIN audiences a (NOLOCK) ON a.id=pa.audience_id
	WHERE 
		pa.proposal_id=@proposal_id
		AND pa.ordinal<>0
	ORDER BY
		pa.ordinal

	-- details
	SELECT
		proposal_details.id,
		n.code,
		spot_lengths.length,
		proposal_details.num_spots,
		proposal_details.topography_universe 'universe',
		proposal_details.proposal_rate
	FROM
		proposal_details (NOLOCK) 
		JOIN proposals p (NOLOCK) ON p.id=proposal_details.proposal_id
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=proposal_details.spot_length_id
		JOIN uvw_network_universe n	(NOLOCK) ON n.network_id=proposal_details.network_id AND (n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL))
	WHERE
		proposal_details.proposal_id=@proposal_id
		AND (proposal_details.num_spots>0 OR proposal_details.include=1)
	ORDER BY
		n.code

	-- audience details
	SELECT
		proposal_details.id,
		proposal_detail_audiences.audience_id,
		proposal_detail_audiences.rating,
		CASE proposals.is_equivalized
			WHEN 0 THEN
				((proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor) * proposal_detail_audiences.rating) / 1000.0
			ELSE
				(((proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor) * proposal_detail_audiences.rating) / 1000.0) * spot_lengths.delivery_multiplier
		END 'delivery'
	FROM
		proposal_detail_audiences (NOLOCK) 
		JOIN proposal_details (NOLOCK) ON proposal_details.id=proposal_detail_audiences.proposal_detail_id
		JOIN proposals (NOLOCK) ON proposals.id=proposal_details.proposal_id
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=proposal_details.spot_length_id
		JOIN audiences (NOLOCK) ON audiences.id=proposal_detail_audiences.audience_id
	WHERE
		proposal_details.proposal_id=@proposal_id
		-- this removes lines with 0 spots which do NOT have the include field enabled
		AND (proposal_details.num_spots>0 OR include=1)
END
