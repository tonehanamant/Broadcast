	
CREATE PROCEDURE usp_TCS_GetMarryCandidateFlight
(
	@proposal_id int,
	@media_month_id int
)
AS
BEGIN
	select 
		pmcf.*
	from
		proposals p WITH (NOLOCK)
		join proposal_marry_candidates pmc WITH (NOLOCK) on pmc.proposal_id = p.id and pmc.media_month_id = @media_month_id
		join proposal_marry_candidate_flights pmcf WITH (NOLOCK) ON  pmc.id = pmcf.proposal_marry_candidate_id
	where
		p.id = @proposal_id
	
	
	END
