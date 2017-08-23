	-- =============================================
	-- Author:		David Sisson
	-- Create date: 03/16/2009
	-- Description:	Returns active network records as of specified date.
	-- =============================================
	CREATE FUNCTION [dbo].[udf_ExpandTurnkeysAsOf]
	(	
		@dateAsOf as datetime
	)
	RETURNS TABLE
	AS
	RETURN 
	(
		select
			tmp.traffic_zone_id traffic_zone_id,
			tmp.invoice_zone_id invoice_zone_id,
			tmp.type
		from
		(
		select
			traffic_zone_id,
			invoice_zone_id,
			'TurnKey' type
		from
			(
				select
					traffic_zones.zone_id traffic_zone_id,
					invoice_zones.zone_id invoice_zone_id
				from
					udf_GetSystemsAsOf(@dateAsOf) systems
					join udf_GetSystemZonesAsOf(@dateAsOf) system_zones on
						systems.system_id = system_zones.system_id
					join udf_GetZonesAsOf(@dateAsOf) invoice_zones on
						invoice_zones.zone_id = system_zones.zone_id
					join udf_GetZonesAsOf(@dateAsOf) traffic_zones on
						traffic_zones.code = '7029'
				where
					systems.code = 'JACKMS'
					and
					system_zones.type = 'BILLING'
					and
					invoice_zones.traffic = 0

				except

				select
					trafficked_zone_maps.traffic_zone_id,
					trafficked_zone_maps.invoice_zone_id
				from
					udf_ExpandZoneGroupsAsOf(@dateAsOf) trafficked_zone_maps
					join udf_GetZonesAsOf(@dateAsOf) zones on
						zones.zone_id = trafficked_zone_maps.traffic_zone_id
			) JACKMS

		union all

		select
			traffic_zone_id,
			invoice_zone_id,
			'TurnKey' type
		from
			(
				select
					traffic_zones.zone_id traffic_zone_id,
					invoice_zones.zone_id invoice_zone_id
				from
					udf_GetSystemsAsOf(@dateAsOf) systems
					join udf_GetSystemZonesAsOf(@dateAsOf) system_zones on
						systems.system_id = system_zones.system_id
					join udf_GetZonesAsOf(@dateAsOf) invoice_zones on
						invoice_zones.zone_id = system_zones.zone_id
					join udf_GetZonesAsOf(@dateAsOf) traffic_zones on
						traffic_zones.code = '7012'
				where
					systems.code = 'NASHTUP'
					and
					system_zones.type = 'BILLING'
					and
					invoice_zones.traffic = 0

				except

				select
					trafficked_zone_maps.traffic_zone_id,
					trafficked_zone_maps.invoice_zone_id
				from
					udf_ExpandZoneGroupsAsOf(@dateAsOf) trafficked_zone_maps
					join udf_GetZonesAsOf(@dateAsOf) zones on
						zones.zone_id = trafficked_zone_maps.traffic_zone_id
			) NASHTUP

		union all

		select
			traffic_zone_id,
			invoice_zone_id,
			'TurnKey' type
		from
			(
				select
					traffic_zones.zone_id traffic_zone_id,
					invoice_zones.zone_id invoice_zone_id
				from
					udf_GetSystemsAsOf(@dateAsOf) systems
					join udf_GetSystemZonesAsOf(@dateAsOf) system_zones on
						systems.system_id = system_zones.system_id
					join udf_GetZonesAsOf(@dateAsOf) invoice_zones on
						invoice_zones.zone_id = system_zones.zone_id
					join udf_GetZonesAsOf(@dateAsOf) traffic_zones on
						traffic_zones.code = '7025'
				where
					systems.code = 'NASHHAT'
					and
					system_zones.type = 'BILLING'
					and
					invoice_zones.traffic = 0

				except

				select
					trafficked_zone_maps.traffic_zone_id,
					trafficked_zone_maps.invoice_zone_id
				from
					udf_ExpandZoneGroupsAsOf(@dateAsOf) trafficked_zone_maps
					join udf_GetZonesAsOf(@dateAsOf) zones on
						zones.zone_id = trafficked_zone_maps.traffic_zone_id
			) NASHHAT

		union all

		select
			traffic_zone_id,
			invoice_zone_id,
			'TurnKey' type
		from
			(
				select
					traffic_zones.zone_id traffic_zone_id,
					invoice_zones.zone_id invoice_zone_id
				from
					udf_GetSystemsAsOf(@dateAsOf) systems
					join udf_GetSystemZonesAsOf(@dateAsOf) system_zones on
						systems.system_id = system_zones.system_id
					join udf_GetZonesAsOf(@dateAsOf) invoice_zones on
						invoice_zones.zone_id = system_zones.zone_id
					join udf_GetZonesAsOf(@dateAsOf) traffic_zones on
						traffic_zones.code = '7087'
				where
					systems.code = 'MEMPTN'
					and
					system_zones.type = 'BILLING'
					and
					invoice_zones.traffic = 0

				except

				select
					trafficked_zone_maps.traffic_zone_id,
					trafficked_zone_maps.invoice_zone_id
				from
					udf_ExpandZoneGroupsAsOf(@dateAsOf) trafficked_zone_maps
					join udf_GetZonesAsOf(@dateAsOf) zones on
						zones.zone_id = trafficked_zone_maps.traffic_zone_id
			) MEMPTN
		) tmp
	where
		traffic_zone_id <> invoice_zone_id
		and
		invoice_zone_id not in (
			select
				primary_zone_id
			from
				udf_GetZoneZonesAsOf(@dateAsOf, 'ZoneGroup')
		)
	);
