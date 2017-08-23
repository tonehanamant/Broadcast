
CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastDetailMarkets]
(
	@broadcast_detail_id int
)
AS
	select 
		*
	from
		broadcast_detail_markets dm WITH (NOLOCK)
	where
		dm.broadcast_proposal_detail_id = @broadcast_detail_id

