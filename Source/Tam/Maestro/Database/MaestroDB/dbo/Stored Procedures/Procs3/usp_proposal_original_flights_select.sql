CREATE PROCEDURE usp_proposal_original_flights_select
(
	@proposal_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	proposal_original_flights WITH(NOLOCK)
WHERE
	proposal_id=@proposal_id
	AND
	start_date=@start_date

