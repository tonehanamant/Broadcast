CREATE PROCEDURE [dbo].[usp_PCS_GetProposalDetailsForCandidate]
(
	@proposal_id int
)
AS
		SELECT  
			pd.id, 
			pd.network_id, 
			pd.daypart_id, 
			pd.proposal_rate, 
			pd.num_spots,
			pdw.units,
			pd.topography_universe,
			pd.universal_scaling_factor,
			spot_lengths.delivery_multiplier,
			proposal_detail_audiences.rating,
			proposal_detail_audiences.us_universe,
			mw.start_date,
			mw.end_date,
			networks.code,
			proposals.rate_card_type_id
		FROM
			proposal_detail_worksheets pdw (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pdw.proposal_detail_id
			JOIN proposals (NOLOCK) on proposals.id = pd.proposal_id
			JOIN uvw_network_universe networks (NOLOCK) ON networks.network_id=pd.network_id AND (networks.start_date<=proposals.start_date AND (networks.end_date>=proposals.start_date OR networks.end_date IS NULL))
			JOIN spot_lengths on spot_lengths.id = pd.spot_length_id
			JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
			JOIN proposal_audiences (NOLOCK) on proposal_audiences.ordinal = proposals.guarantee_type and proposal_audiences.proposal_id = proposals.id
			JOIN proposal_detail_audiences (NOLOCK) on proposal_detail_audiences.proposal_detail_id = pd.id 
				and proposal_detail_audiences.audience_id = proposal_audiences.audience_id
		WHERE
			pd.proposal_id=@proposal_id 
		ORDER BY
			pd.id, 
			pd.network_id, 
			pd.daypart_id,
			mw.start_date

