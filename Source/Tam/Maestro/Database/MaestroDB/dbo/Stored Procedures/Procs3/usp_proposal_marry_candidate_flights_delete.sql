CREATE PROCEDURE usp_proposal_marry_candidate_flights_delete
(
	@proposal_marry_candidate_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	proposal_marry_candidate_flights
WHERE
	proposal_marry_candidate_id = @proposal_marry_candidate_id
 AND
	start_date = @start_date
