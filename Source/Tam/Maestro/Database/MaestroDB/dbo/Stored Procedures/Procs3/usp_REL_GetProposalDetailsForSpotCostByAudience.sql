-- usp_REL_GetProposalDetailsForSpotCostByAudience 48487,58,35769
CREATE PROCEDURE [dbo].[usp_REL_GetProposalDetailsForSpotCostByAudience]
(
	@proposal_id Int,
	@audience_id int,
	@traffic_id int
)
AS
BEGIN
	select 
		pd.network_id, 
		pd.daypart_id, 
		pda.audience_id,
		CASE
			WHEN 
				(pda.rating * pda.us_universe * pd.universal_scaling_factor) = 0 
			THEN 
				0
			ELSE 
				pd.proposal_rate / ( (pda.rating * pda.us_universe * pd.universal_scaling_factor) / 1000.0 )
		END [CPM],
		pda.rating, 
		pd.proposal_rate, 
		dbo.GetProposalDetailCoverageUniverse(pd.id, 31) [Proposal HH Universe],
		case when traffic_detail_audiences.traffic_rating is null then 0.0 else traffic_detail_audiences.traffic_rating end,
		dbo.GetProposalDetailCoverageUniverse(pd.id, pda.audience_id) [Proposal Universe For CPM],
		dbo.GetTrafficDetailCoverageUniverse(traffic_details.id, traffic_detail_audiences.audience_id, 1) AS Traf_Prim_Universe,
		spot_lengths.delivery_multiplier,
		traffic_details.id
	from 
		proposal_details pd (NOLOCK) 
		join proposal_detail_audiences pda (NOLOCK) on pd.id = pda.proposal_detail_id 
		join proposals (NOLOCK) on proposals.id = pd.proposal_id 
		join proposal_audiences pa (NOLOCK) on pa.audience_id = pda.audience_id 
			and pa.proposal_id = proposals.id  
		join traffic_details (NOLOCK) on traffic_details.network_id = pd.network_id
		join spot_lengths (NOLOCK) on pd.spot_length_id = spot_lengths.id
		left join traffic_detail_audiences (NOLOCK) on traffic_detail_audiences.traffic_detail_id = traffic_details.id 
			and traffic_detail_audiences.audience_id = pa.audience_id
	where 
		pd.proposal_id = @proposal_id 
		and traffic_details.traffic_id = @traffic_id 
		and pa.audience_id = @audience_id 
END