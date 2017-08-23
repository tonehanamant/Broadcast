CREATE PROCEDURE [dbo].[usp_REL_GetZonesForTrafficIDAndSystemID]
	@system_id int,
	@traffic_id int
AS

select 
	distinct zone_id 
from 
	traffic_orders (NOLOCK) 
	join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
where 
	traffic_details.traffic_id = @traffic_id 
	and traffic_orders.system_id = @system_id
	and traffic_orders.active = 1

