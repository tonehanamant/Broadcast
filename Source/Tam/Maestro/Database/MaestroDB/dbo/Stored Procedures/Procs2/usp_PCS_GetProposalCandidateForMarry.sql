
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalCandidateForMarry]
	@proposal_id int,
	@media_month_id int
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
		proposals.rate_card_type_id,
		proposals.base_ratings_media_month_id,
		proposals.base_universe_media_month_id,
		pmc.id
	from 
		proposals (NOLOCK)
		join proposal_marry_candidates (NOLOCK) pmc on pmc.proposal_id = proposals.id 
		join proposal_audiences (NOLOCK) on proposals.id = proposal_audiences.proposal_id 
		and cast(proposals.guarantee_type as bit) = proposal_audiences.ordinal 
	where
		proposals.id = @proposal_id
		and pmc.media_month_id = @media_month_id
END
