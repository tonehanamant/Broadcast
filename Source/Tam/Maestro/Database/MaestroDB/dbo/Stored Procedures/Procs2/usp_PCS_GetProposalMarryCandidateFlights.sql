
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalMarryCandidateFlights]
(
	@proposal_marry_candidate_id int
)
AS

SELECT
	proposal_marry_candidate_id,
	start_date,
	end_date,
	selected
FROM
	proposal_marry_candidate_flights WITH (NOLOCK)
WHERE
	proposal_marry_candidate_id = @proposal_marry_candidate_id;
