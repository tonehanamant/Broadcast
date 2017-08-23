
CREATE PROCEDURE [dbo].[usp_PCS_RemoveProposalAsMarryCandidate]
(
	@proposal_id int,
	@spot_length_id int,
	@media_month_id int
)
AS

DELETE FROM proposal_marry_candidate_flights where 
proposal_marry_candidate_id in
(
	SELECT ID FROM proposal_marry_candidates WITH (NOLOCK)
	WHERE
		proposal_id = @proposal_id and
		media_month_id = @media_month_id and
		spot_length_id = @spot_length_id
);

DELETE FROM proposal_marry_candidates
WHERE
	proposal_id = @proposal_id and
	media_month_id = @media_month_id and
	spot_length_id = @spot_length_id;
	

