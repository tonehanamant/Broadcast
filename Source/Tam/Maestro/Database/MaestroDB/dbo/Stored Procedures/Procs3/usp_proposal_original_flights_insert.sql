CREATE PROCEDURE usp_proposal_original_flights_insert
(
	@proposal_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
INSERT INTO proposal_original_flights
(
	proposal_id,
	start_date,
	end_date,
	selected
)
VALUES
(
	@proposal_id,
	@start_date,
	@end_date,
	@selected
)

