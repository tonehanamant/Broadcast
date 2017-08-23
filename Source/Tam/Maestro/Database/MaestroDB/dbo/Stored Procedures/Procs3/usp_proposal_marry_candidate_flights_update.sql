
CREATE PROCEDURE usp_proposal_marry_candidate_flights_update
(
	@proposal_marry_candidate_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
UPDATE proposal_marry_candidate_flights SET
	end_date = @end_date,
	selected = @selected
WHERE
	proposal_marry_candidate_id = @proposal_marry_candidate_id AND
	start_date = @start_date

