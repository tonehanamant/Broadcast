
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_REL_GetAllTraffickableSystemsForTopographies]
(
	@effective_date datetime,
	@topographyIds UniqueIdTable READONLY
)
AS

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

SELECT 
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
	systems.order_regeneration_traffic_alert ,
	systems.custom_traffic_system 
FROM 
	dbo.udf_GetTrafficZoneInformationByTopographiesAsOf(@topographyIds, @effective_date, 1) traffic_systems
join
	systems on systems.id = traffic_systems.system_id
GROUP BY --FASTER THAN DISTINCT
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
order by
	systems.code

if OBJECT_ID('[dbo].[usp_REL_GetAllTraffickableSystemsForTopographies]') IS NULL
begin
  print 'ERROR: usp_REL_GetAllTraffickableSystemsForTopographies NOT created.'
end
