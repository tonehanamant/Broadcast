
CREATE PROCEDURE [dbo].[usp_TCS_GetNumberOfTrafficLineItems]
(
            @traffic_detail_id as int
)

AS

with myview as
(select distinct traffic_details.network_id, traffic_details.daypart_id, traffic_detail_weeks.start_date 

from traffic_details (NOLOCK)
	join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id
	join traffic_detail_topographies (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
where 
	traffic_details.id = @traffic_detail_id )

select count(*) from myview

