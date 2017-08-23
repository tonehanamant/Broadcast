
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE Procedure [dbo].[usp_REL_GetDistinctSystemsForTraffic]
      (
            @traffic_id int
      )

AS

select 
      distinct 
      systems.id, 
      systems.code, 
      systems.name, 
      systems.location, 
      systems.spot_yield_weight, 
      systems.traffic_order_format, 
      systems.flag, 
      systems.active, 
      systems.effective_date,
	  systems.generate_traffic_alert_excel,
	  systems.one_advertiser_per_traffic_alert,
	  systems.cancel_recreate_order_traffic_alert,
	  systems.order_regeneration_traffic_alert,
	  systems.custom_traffic_system   
from traffic_orders (NOLOCK) 
      join systems (NOLOCK) on systems.id = traffic_orders.system_id 
      join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
      join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
WHERE
      traffic_details.traffic_id = @traffic_id 
      and traffic_orders.on_financial_reports = 1
      and traffic_orders.active = 1
      and 
      (
            traffic_detail_weeks.suspended = 0
      )
ORDER BY systems.code
