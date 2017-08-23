CREATE PROCEDURE [dbo].[usp_PCS_GetProposalDetailsForMarriageReport]
(
	@proposal_id int,
	@start_date datetime,
	@end_date datetime
)
AS
		SELECT DISTINCT 
			pd.id, 
			pd.network_id, 
			pd.daypart_id, 
			pd.proposal_rate, 
			SUM(pdw.units),
			pd.topography_universe,
			pd.universal_scaling_factor,
			spot_lengths.delivery_multiplier,
			proposal_detail_audiences.rating,
			proposal_detail_audiences.us_universe
		FROM
			proposal_detail_worksheets pdw (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pdw.proposal_detail_id
			JOIN proposals (NOLOCK) on proposals.id = pd.proposal_id
			JOIN spot_lengths on spot_lengths.id = pd.spot_length_id
			JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
			JOIN proposal_audiences (NOLOCK) on proposal_audiences.ordinal = proposals.guarantee_type and proposal_audiences.proposal_id = proposals.id
			JOIN proposal_detail_audiences (NOLOCK) on proposal_detail_audiences.proposal_detail_id = pd.id 
				and proposal_detail_audiences.audience_id = proposal_audiences.audience_id
		WHERE
			pd.proposal_id=@proposal_id
			AND (mw.start_date <= @end_date AND mw.end_date >= @start_date)
		GROUP BY
			pd.id, 
			pd.network_id, 
			pd.daypart_id, 
			pd.proposal_rate,
			pd.topography_universe,
			pd.universal_scaling_factor,
			spot_lengths.delivery_multiplier,
			proposal_detail_audiences.rating,
			proposal_detail_audiences.us_universe

