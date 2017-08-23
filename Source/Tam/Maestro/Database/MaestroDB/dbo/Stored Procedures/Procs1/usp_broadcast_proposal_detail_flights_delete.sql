CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_flights_delete]
(
	@broadcast_proposal_detail_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	broadcast_proposal_detail_flights
WHERE
	broadcast_proposal_detail_id = @broadcast_proposal_detail_id
 AND
	start_date = @start_date


