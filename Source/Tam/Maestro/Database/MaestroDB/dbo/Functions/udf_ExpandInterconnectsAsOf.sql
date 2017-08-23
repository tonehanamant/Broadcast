	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 02/13/2015
	-- Description:	
	-- =============================================
	CREATE FUNCTION [dbo].[udf_ExpandInterconnectsAsOf]
	(
		@dateAsOf as datetime
	)
	RETURNS TABLE
	AS
	RETURN 
	(
		with
		zone_group_zones(
			traffic_zone_id,
			invoice_zone_id,
			type
		) as (
			select	
				zone_zone.primary_zone_id traffic_zone_id,
				zone_zone.secondary_zone_id invoice_zone_id,
				zone_zone.type
			from
				udf_GetZoneZonesAsOf(@dateAsOf, 'Interconnect') zone_zone
			union all
			select
				zone_group_zones.traffic_zone_id traffic_zone_id,
				zone_zone.secondary_zone_id invoice_zone_id,
				zone_group_zones.type
			from
				zone_group_zones
				join udf_GetZoneZonesAsOf(@dateAsOf, 'Interconnect') zone_zone on
					zone_zone.primary_zone_id = zone_group_zones.invoice_zone_id
		)
		select
			zone_traffic.traffic_zone_id traffic_zone_id,
			zone_traffic.invoice_zone_id invoice_zone_id,
			zone_traffic.type
		from
			zone_group_zones zone_traffic
		where
			zone_traffic.traffic_zone_id <> zone_traffic.invoice_zone_id
	);
