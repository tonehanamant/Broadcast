CREATE PROCEDURE usp_proposal_original_flights_select_all
AS
SELECT
	*
FROM
	proposal_original_flights WITH(NOLOCK)
