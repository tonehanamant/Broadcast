CREATE PROCEDURE usp_proposal_marry_candidate_flights_insert
(
	@proposal_marry_candidate_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS

INSERT INTO proposal_marry_candidate_flights
(
	proposal_marry_candidate_id,
	start_date,
	end_date,
	selected
)
VALUES
(
	@proposal_marry_candidate_id,
	@start_date,
	@end_date,
	@selected
)

