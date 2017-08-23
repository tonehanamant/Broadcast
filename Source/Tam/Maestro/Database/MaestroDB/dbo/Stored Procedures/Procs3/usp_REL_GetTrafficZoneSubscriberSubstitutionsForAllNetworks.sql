


CREATE PROCEDURE [dbo].[usp_REL_GetTrafficZoneSubscriberSubstitutionsForAllNetworks]
	 
AS
declare @date datetime;
declare @dateEndOfTime datetime;
declare @dateBeginingOfTime datetime;
declare @nameDMAUnidentified varchar;
declare @codeDMAUnidentified varchar;
declare @idDMAUnidentified int;

set @date = getdate();

select
	@dateEndOfTime = cast(value as datetime)
from
	dbo.properties
where
	name = 'end_of_time';

select
	@dateBeginingOfTime = cast(value as datetime)
from
	dbo.properties
where
	name = 'begining_of_time';

set @nameDMAUnidentified = 'UNIDENTIFIED';

select
	@idDMAUnidentified = id,
	@codeDMAUnidentified = code
from
	dmas
where
	name = @nameDMAUnidentified;

with
zones(
	zone_id, code
) as (
	select
		zones.zone_id, zones.code
	from
		dbo.uvw_zone_universe zones (NOLOCK)
		join dbo.uvw_zonedma_universe zone_dmas (NOLOCK) on
			zones.zone_id = zone_dmas.zone_id
		join dbo.uvw_dma_universe dmas (NOLOCK) on
			dmas.dma_id = zone_dmas.dma_id
	where
		-- Zone filters
		@date between zones.start_date and isnull(zones.end_date, @dateEndOfTime)
		--and zones.zone_id in (
		--	select id from dbo.GetZonesInTopographyByDate(@idTopography, @date)
		--)
		and zones.traffic = 1
		and zones.active = 1
		-- Zone DMA filters
		and @date between zone_dmas.start_date and isnull(zone_dmas.end_date, @dateEndOfTime)
		and 0.5 < zone_dmas.weight
)


select zone_networks.zone_id, network_maps.network_id [source_network_id], network_maps.map_value, networks.id [dest_network_id], networks.code, 
zone_networks.subscribers 
	from network_maps (NOLOCK) 
	join networks (NOLOCK) on networks.code = network_maps.map_value
	join zone_networks (NOLOCK) on zone_networks.network_id = networks.id 
	join zones on zones.zone_id = zone_networks.zone_id
where network_maps.map_set='traffic' and zone_networks.trafficable = 1 and zone_networks.effective_date <= getdate() 
and networks.active=1
order by zone_networks.zone_id, network_maps.network_id

