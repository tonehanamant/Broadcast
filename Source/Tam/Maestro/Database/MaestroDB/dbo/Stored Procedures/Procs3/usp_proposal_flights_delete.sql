CREATE PROCEDURE usp_proposal_flights_delete
(
	@proposal_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	proposal_flights
WHERE
	proposal_id = @proposal_id
 AND
	start_date = @start_date
