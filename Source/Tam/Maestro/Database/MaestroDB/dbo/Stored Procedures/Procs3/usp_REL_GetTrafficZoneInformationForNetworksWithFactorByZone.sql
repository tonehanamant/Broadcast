CREATE PROCEDURE [dbo].[usp_REL_GetTrafficZoneInformationForNetworksWithFactorByZone]
	 @idTopography as int,
	 @release_id as int,
	 @zone_id as int
AS

declare
	@date as datetime;

--select @date = min(traffic.start_date) from traffic 
--	where traffic.release_id = @release_id;

select @date = releases.release_date from releases 
	where releases.id = @release_id;

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
where 
		zone_id = @zone_id
order by
       system_id,
       zone_id,
       traffic_network_id,
       zone_network_id;

