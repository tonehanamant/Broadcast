



CREATE Procedure [dbo].[usp_REL_GetZonesForTrafficOrdersInRelease]
(
	@traffic_ids varchar(max)
)
AS

declare @query as varchar(max);

set @query = 'select 
	DISTINCT 
	zones.id,
	zones.code, 
	zones.[name], 
	zones.[type], 
	zones.[primary], 
	zones.traffic, 
	zones.dma,
	zones.flag,
 	zones.active,
	zones.effective_date, 
	count(distinct traffic_details.traffic_id)
from traffic_orders WITH (NOLOCK) 
join zones WITH (NOLOCK) on traffic_orders.zone_id = zones.id
join traffic_details WITH (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
join traffic_detail_weeks WITH (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
where 
	traffic_details.traffic_id in (' + @traffic_ids + ')
	and traffic_detail_weeks.suspended = 0 and traffic_orders.active = 1
GROUP BY 
	zones.id,
	zones.code, 
	zones.[name], 
	zones.[type], 
	zones.[primary], 
	zones.traffic, 
	zones.dma,
	zones.flag,
 	zones.active,
	zones.effective_date
order by zones.code';

print @query;
exec (@query);

