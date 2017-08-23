CREATE Procedure [dbo].[usp_REL_GetTrafficSpotsForAllSystemsAllTopographies]
      (
            @traffic_id int
	  )

AS

select 
	distinct 
	networks.code, 
	traffic_details.network_id, 
	traffic_orders.daypart_id, 
	traffic_orders.traffic_detail_id,
	traffic_orders.start_date, 
	traffic_orders.end_date,
	count(distinct traffic_orders.system_id)
from traffic_orders WITH (NOLOCK)
	join traffic_details WITH (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id 
	join traffic_detail_weeks WITH (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	join traffic_detail_topographies WITH (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id 
	join traffic WITH (NOLOCK) on traffic.id = traffic_details.traffic_id 	
	join uvw_network_universe networks WITH (NOLOCK) on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
where 
	traffic_details.traffic_id = @traffic_id 
	and traffic_detail_weeks.suspended = 0
	and traffic_orders.on_financial_reports = 1
	and traffic_orders.active = 1
group by
	networks.code, 
	traffic_details.network_id, 
	traffic_orders.daypart_id, 
	traffic_orders.traffic_detail_id,
	traffic_orders.start_date, 
	traffic_orders.end_date
order by 
	networks.code, 
	traffic_orders.start_date, 
	count(distinct traffic_orders.system_id) DESC,
	traffic_orders.daypart_id
	
