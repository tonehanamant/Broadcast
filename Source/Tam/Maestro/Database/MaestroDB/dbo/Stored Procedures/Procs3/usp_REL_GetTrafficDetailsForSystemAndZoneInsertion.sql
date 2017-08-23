CREATE PROCEDURE [dbo].[usp_REL_GetTrafficDetailsForSystemAndZoneInsertion]
      @traffic_id as int,
      @zone_id as int,
      @system_id as int
AS

select 
      traffic_orders.traffic_detail_id, 
      traffic_orders.subscribers, 
      nielsen_networks.code, 
      traffic_orders.daypart_id, 
      traffic_orders.start_date, 
      traffic_orders.end_date,
      traffic_orders.ordered_spot_rate, 
      traffic_orders.ordered_spots, 
      nielsen_networks.nielsen_id, 
      nielsen_networks.name,
      traffic_details.spot_length_id, 
      spot_lengths.length, 
      traffic_orders.id,
      ts.start_time,
      ts.end_time
from 
      traffic_orders (NOLOCK) 
      join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
        join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
      join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
      join spot_lengths (NOLOCK) on spot_lengths.id = traffic_details.spot_length_id 
      join dayparts d (NOLOCK) on d.id = traffic_orders.daypart_id
      join timespans ts (NOLOCK) on ts.id = d.timespan_id
      left join uvw_networkmap_universe (NOLOCK) network_maps on network_maps.network_id = traffic_orders.display_network_id and network_maps.map_set = 'Nielsen' AND (network_maps.start_date<=traffic.start_date AND (network_maps.end_date>=traffic.start_date OR network_maps.end_date IS NULL))
      left join uvw_nielsen_network_universes (NOLOCK) nielsen_networks on nielsen_networks.nielsen_id = cast(network_maps.map_value as int)
                  and (nielsen_networks.start_date<=traffic.start_date AND (nielsen_networks.end_date>=traffic.start_date OR nielsen_networks.end_date IS NULL))
where 
      traffic_orders.zone_id = @zone_id 
      and traffic.id = @traffic_id 
      and traffic_orders.system_id = @system_id 
      and traffic_orders.ordered_spots > 0
        and traffic_detail_weeks.suspended = 0
        and traffic_orders.active = 1
order by 
	  nielsen_networks.code,
      traffic_orders.start_date
