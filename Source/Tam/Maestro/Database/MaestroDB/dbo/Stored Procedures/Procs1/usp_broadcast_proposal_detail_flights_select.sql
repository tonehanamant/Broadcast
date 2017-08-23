CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_flights_select]
(
	@broadcast_proposal_detail_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	broadcast_proposal_detail_flights WITH(NOLOCK)
WHERE
	broadcast_proposal_detail_id=@broadcast_proposal_detail_id
	AND
	start_date=@start_date

