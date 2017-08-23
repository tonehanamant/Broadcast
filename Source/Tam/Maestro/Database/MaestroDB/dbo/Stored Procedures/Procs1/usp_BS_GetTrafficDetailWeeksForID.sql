

CREATE PROCEDURE usp_BS_GetTrafficDetailWeeksForID
(
	@broadcast_proposal_traffic_id int
)
AS

select * 
from
	broadcast_traffic_detail_weeks tdw with (NOLOCK)
where
	tdw.broadcast_traffic_detail_id = @broadcast_proposal_traffic_id
order by
	tdw.start_date
