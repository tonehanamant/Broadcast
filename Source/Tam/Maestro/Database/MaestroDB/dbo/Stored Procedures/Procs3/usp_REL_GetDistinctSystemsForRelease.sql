

/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/

/* ------------------------------------------------------------------------- */
/* BEGIN Traffic Performance Enhancements                                    */
/* ------------------------------------------------------------------------- */
-- This was hotfixed in production. The query is for legacy traffic, filtering was added
-- to ignore data connected to traffic.plan_type <> 0
CREATE Procedure [dbo].[usp_REL_GetDistinctSystemsForRelease]
(
	@release_id int
)
AS
	select
	distinct systems.id, 
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
	from traffic with (NOLOCK)
	join traffic_orders with (NOLOCK) on traffic_orders.traffic_id = traffic.id
	join systems with (NOLOCK) on systems.id = traffic_orders.system_id 
	join traffic_details with (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
	join traffic_detail_weeks with (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	where traffic_orders.release_id = @release_id
	and traffic.release_id = @release_id
	and traffic_orders.on_financial_reports = 1
	and traffic.plan_type = 0
	and traffic_orders.active = 1
	and traffic_orders.ordered_spots > 0
	and traffic_detail_weeks.suspended = 0
	and media_month_id in (
	select id from uvw_release_media_months
	where release_id = @release_id
)
order by
systems.code
