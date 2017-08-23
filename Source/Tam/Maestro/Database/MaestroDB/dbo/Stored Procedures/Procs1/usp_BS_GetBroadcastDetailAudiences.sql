

CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastDetailAudiences]
(
	@broadcast_detail_id int
)
AS
	select 
		*
	from
		broadcast_proposal_detail_audiences dm WITH (NOLOCK)
	where
		dm.broadcast_proposal_detail_id = @broadcast_detail_id
	order by
		dm.ordinal
