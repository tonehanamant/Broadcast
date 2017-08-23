CREATE PROCEDURE [dbo].[usp_REL_GetTrafficZoneInformationForAllNetworksWithFactorByTraffic]
	 @idTopography as int,
	 @traffic_id as int
AS

declare
	@date as datetime;

--select @date = min(traffic.start_date) from traffic 
--	where traffic.release_id = @release_id;

select @date = min(traffic_orders.start_date) 
from traffic_orders (NOLOCK) join traffic_details (NOLOCK) 
on traffic_orders.traffic_detail_id = traffic_details.id 
where traffic_details.traffic_id = @traffic_id and traffic_orders.ordered_spots > 0;

if(@date is NULL)
BEGIN
select @date = traffic.start_date 
from traffic (NOLOCK)
where traffic.id = @traffic_id;
END

select
       system_id,
       zone_id,
       traffic_network_id,
       zone_network_id,
       subscribers,
       traffic_factor,
       spot_yield_weight,
       on_financial_reports,
	   no_cents_in_spot_rate
from
       dbo.udf_GetTrafficZoneInformationByTopographyAsOf(@idTopography, @date, 1)
order by
       system_id,
       zone_id,
       traffic_network_id,
       zone_network_id;

