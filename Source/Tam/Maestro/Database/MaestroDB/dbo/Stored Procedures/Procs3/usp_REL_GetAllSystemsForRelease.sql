
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE Procedure [dbo].[usp_REL_GetAllSystemsForRelease]
(
    @release_id int
)
AS
select
    distinct traffic_orders.system_id, 
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
from
    traffic_orders WITH (NOLOCK) 
join
    systems WITH (NOLOCK) 
    on traffic_orders.system_id = systems.id
where
    release_id = @release_id 
	and media_month_id in (
		select id from uvw_release_media_months
		where release_id = @release_id)
order by
    systems.code
