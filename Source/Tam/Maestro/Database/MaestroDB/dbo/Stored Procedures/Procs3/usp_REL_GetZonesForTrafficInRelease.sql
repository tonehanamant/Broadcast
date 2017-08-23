
CREATE Procedure [dbo].[usp_REL_GetZonesForTrafficInRelease]  
(  
 @traffic_id int  
)  
AS  
  
select distinct   
 traffic_orders.zone_id,  
 zones.code,   
 zones.[name],   
 zones.[type],   
 zones.[primary],   
 zones.traffic,   
 zones.dma,  
 zones.flag,  
  zones.active,  
 zones.effective_date   
from   
 traffic_orders WITH (NOLOCK)   
 join zones WITH (NOLOCK) on traffic_orders.zone_id = zones.id  
 join traffic_details WITH (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id  
 join traffic_detail_weeks WITH (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
	and traffic_detail_weeks.suspended = 0
where   
 traffic_details.traffic_id = @traffic_id and traffic_orders.active = 1  
 and traffic_orders.on_financial_reports = 1
order by   
 zones.code
