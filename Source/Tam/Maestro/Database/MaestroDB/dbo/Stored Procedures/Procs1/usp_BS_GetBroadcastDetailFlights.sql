
CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastDetailFlights]
(
	@broadcast_detail_id int
)
AS
	select 
		*
	from
		broadcast_proposal_detail_flights bf WITH (NOLOCK)
	where
		bf.broadcast_proposal_detail_id = @broadcast_detail_id

