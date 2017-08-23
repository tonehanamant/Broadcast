
CREATE PROCEDURE usp_proposal_marry_candidate_flights_select
(
	@proposal_marry_candidate_id		Int,
	@start_date		DateTime
)
AS
SELECT
	proposal_marry_candidate_id,
	start_date,
	end_date,
	selected
FROM
	proposal_marry_candidate_flights (NOLOCK)
WHERE
	proposal_marry_candidate_id=@proposal_marry_candidate_id
	AND
	start_date=@start_date

