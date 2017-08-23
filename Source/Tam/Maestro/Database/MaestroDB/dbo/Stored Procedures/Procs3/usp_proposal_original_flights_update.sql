CREATE PROCEDURE usp_proposal_original_flights_update
(
	@proposal_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
UPDATE proposal_original_flights SET
	end_date = @end_date,
	selected = @selected
WHERE
	proposal_id = @proposal_id AND
	start_date = @start_date
