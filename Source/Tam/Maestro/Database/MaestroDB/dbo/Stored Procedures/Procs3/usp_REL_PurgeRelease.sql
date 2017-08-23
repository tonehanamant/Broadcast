
CREATE PROCEDURE [dbo].[usp_REL_PurgeRelease]
	@release_id int
AS

begin
update traffic_detail_weeks set suspended = 0 
from traffic_detail_weeks (NOLOCK)
join traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
where traffic_details.traffic_id in
(select id from traffic (NOLOCK) where release_id = @release_id);

update traffic_orders set release_id = null where traffic_orders.release_id = @release_id;

end;

