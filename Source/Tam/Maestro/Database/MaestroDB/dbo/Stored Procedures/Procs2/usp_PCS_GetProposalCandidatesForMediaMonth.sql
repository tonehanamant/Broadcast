
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalCandidatesForMediaMonth]
	@media_month_id int,
	@spot_length_id int 	
AS
BEGIN
	select 
		proposals.advertiser_company_id, 
		proposals.product_id,    
		proposals.id, 
		proposals.original_proposal_id, 
		proposals.version_number, 
		proposals.name,   
		proposals.flight_text,
		proposals.agency_company_id,
		proposals.is_equivalized,
		proposal_audiences.audience_id,
		a.code,
		proposals.rate_card_type_id,
		proposals.base_ratings_media_month_id,
		proposals.base_universe_media_month_id,
		proposal_statuses.name,
		pmc.id,
		pmc.base_index
	from 
		proposals (NOLOCK)
		join proposal_statuses WITH (NOLOCK) on proposal_statuses.id = proposals.proposal_status_id 
		join proposal_marry_candidates (NOLOCK) pmc on pmc.proposal_id = proposals.id 
		join proposal_audiences (NOLOCK) on proposals.id = proposal_audiences.proposal_id 
		and cast(proposals.guarantee_type as bit) = proposal_audiences.ordinal 
		join audiences a (NOLOCK) ON a.id=proposal_audiences.audience_id
	where
		pmc.spot_length_id = @spot_length_id
		and pmc.media_month_id = @media_month_id 
	order by
		proposals.agency_company_id
END
