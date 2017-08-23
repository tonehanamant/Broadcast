CREATE PROCEDURE usp_proposal_flights_select_all
AS
SELECT
	*
FROM
	proposal_flights WITH(NOLOCK)
