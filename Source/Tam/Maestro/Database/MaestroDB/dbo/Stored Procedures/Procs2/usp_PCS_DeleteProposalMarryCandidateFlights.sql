
CREATE PROCEDURE [dbo].[usp_PCS_DeleteProposalMarryCandidateFlights]
(
	@proposal_marry_candidate_id int
)
AS

DELETE FROM proposal_marry_candidate_flights where 
proposal_marry_candidate_id = @proposal_marry_candidate_id;

