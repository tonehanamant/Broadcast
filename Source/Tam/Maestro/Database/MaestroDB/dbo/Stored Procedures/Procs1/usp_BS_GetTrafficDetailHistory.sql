
CREATE PROCEDURE usp_BS_GetTrafficDetailHistory
(
	@broadcast_proposal_detail_id int,
	@system_id int,
	@zone_id int
)
AS

declare @orig_id int;

select @orig_id = original_broadcast_proposal_detail_id from broadcast_proposal_details with (NOLOCK) where
id = @broadcast_proposal_detail_id;

select 
	distinct btd.* 
from
	broadcast_traffic_details btd with (NOLOCK)
	join broadcast_proposal_details d1 with (NOLOCK) on d1.id = btd.broadcast_proposal_detail_id
where
	d1.original_broadcast_proposal_detail_id = @orig_id
	and
	btd.system_id = @system_id
	and
	btd.zone_id = @zone_id
order by
	btd.revision
