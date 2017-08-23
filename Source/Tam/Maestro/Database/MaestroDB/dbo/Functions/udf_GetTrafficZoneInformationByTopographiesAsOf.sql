CREATE FUNCTION [dbo].[udf_GetTrafficZoneInformationByTopographiesAsOf]
(     
      @topographyIds as UniqueIdTable READONLY,
      @dateAsOf as datetime,
      @is_trafficakable bit
)
RETURNS @zone_info TABLE
(
	topography_id int,
	system_id int,
	zone_id int,
	traffic_network_id int,
	zone_network_id int,
	subscribers int,
	traffic_factor float,
	spot_yield_weight float,
	on_financial_reports bit,
	no_cents_in_spot_rate bit
)
AS
BEGIN

	DECLARE @tblNetworkMaps table(network_map_id int, network_id int, map_set varchar(15), map_value varchar(63), flag tinyint, as_of_date datetime)
	
	insert into @tblNetworkMaps
	Select [network_map_id],
			[network_id], 
			[map_set], 
			[map_value], 
			[flag],
			[as_of_date]
	from udf_GetNetworkMapsAsOf(@dateAsOf)
	
	DECLARE @tblNetworks TABLE(network_id int, code varchar(15), name varchar(63), flag tinyint, as_of_date datetime)

	insert into @tblNetworks
	Select [network_id], 
			[code], 
			[name], 
			[flag],
			[as_of_date]
	from udf_GetNetworksAsOf(@dateAsOf);
	
	
	DECLARE @network_zones TABLE (traffic_network_id int, zone_network_id int)
	
	insert into @network_zones
	select
                  tn.network_id [traffic_network_id],
                  n.network_id [zone_network_id]
            from
                  @tblNetworks n
                  join @tblNetworkMaps nm on
                        nm.map_set = 'TRAFFIC'
                        and
                        n.code = nm.map_value
                  join @tblNetworks tn on
                        tn.network_id = nm.network_id
            union
            select
                  tn.network_id [traffic_network_id],
                  n.network_id [zone_network_id]
            from
                  @tblNetworks n
                  join @tblNetworkMaps nm on
                        nm.map_set = 'DaypartNetworks'
                        and
                        n.network_id = cast(nm.map_value as int)
                  join @tblNetworks tn on
                        tn.network_id = nm.network_id
                        
      insert into @zone_info
      (
		topography_id,
		system_id,
		zone_id,
		traffic_network_id,
		zone_network_id,
		subscribers,
		traffic_factor,
		spot_yield_weight,
		on_financial_reports,
		no_cents_in_spot_rate
	  )
      select
			s.topography_id,
            s.system_id [system_id],
            z.zone_id [zone_id],
            isnull(nm.traffic_network_id, n.network_id) [traffic_network_id],
            n.network_id [zone_network_id],
            zn.subscribers [subscribers],
            isnull(ct.traffic_factor, 1.0) [traffic_factor],
            s.spot_yield_weight [spot_yield_weight],
            cast(
                  case s.traffic_order_format & 0x00040000
                        when 0x00040000 then 1
                        else 0
                  end as bit
            ) on_financial_reports,
			cast(
				case s.traffic_order_format & 0x00100000
					when 0x00100000 then 1
					else 0
				end as bit
			) no_cents_in_spot_rate
      from
            dbo.udf_GetSystemsByTopographiesAsOf(@topographyIds, @dateAsOf) s
            join dbo.udf_GetSystemZonesAsOf(@dateAsOf) sz on
                  s.system_id = sz.system_id
            join dbo.udf_GetZonesInTopographiesByDate(@topographyIds, @dateAsOf) tz on
                  tz.id = sz.zone_id
            join dbo.udf_GetZonesAsOf(@dateAsOf) z on
                  z.zone_id = tz.id
            join udf_GetZoneNetworksAsOf(@dateAsOf) zn on
                  z.zone_id = zn.zone_id
            join @tblNetworks n on
                  n.network_id = zn.network_id
            left join @network_zones nm on
                  n.network_id = nm.zone_network_id
            left join dbo.udf_GetCustomTrafficAsOf(@dateAsOf) ct on
                  s.system_id = ct.system_id and z.zone_id = ct.zone_id
      where
            sz.type = 'TRAFFIC'
            and
            z.traffic = @is_trafficakable
            and
            zn.trafficable = @is_trafficakable
            
     RETURN;
END
