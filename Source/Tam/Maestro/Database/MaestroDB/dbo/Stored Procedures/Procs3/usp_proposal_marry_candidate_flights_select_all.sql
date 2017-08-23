CREATE PROCEDURE usp_proposal_marry_candidate_flights_select_all
AS
SELECT
	proposal_marry_candidate_id,
	start_date,
	end_date,
	selected
FROM
	proposal_marry_candidate_flights (NOLOCK)
