
CREATE PROCEDURE [dbo].[usp_REL_GetDistinctZonesForSystemInRelease]
(
	@release_id int,
	@system_id int
)
AS


	select 
		distinct 
		systems.code,
		zones.code, 
		zones.name,
		traffic_orders.on_financial_reports
	from 
		traffic_orders (NOLOCK) 
		join zones (NOLOCK) on zones.id = traffic_orders.zone_id 
		join systems (NOLOCK) on systems.id = traffic_orders.system_id 
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
	where 
		traffic_orders.release_id = @release_id 
		and traffic_orders.system_id = @system_id  
		and traffic_orders.active = 1
	order by 
		zones.code
