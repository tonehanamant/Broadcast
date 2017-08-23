CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_flights_update]
(
	@broadcast_proposal_detail_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
UPDATE broadcast_proposal_detail_flights SET
	end_date = @end_date,
	selected = @selected
WHERE
	broadcast_proposal_detail_id = @broadcast_proposal_detail_id AND
	start_date = @start_date
