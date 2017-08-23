	-- =============================================
	-- Author:      Stephen DeFusco
	-- Create date: 11/5/2010
	-- Description: 
	-- =============================================
	CREATE FUNCTION [dbo].[udf_TrafficToBillingSystems]
	(
		@effective_date DATETIME
	)
	RETURNS @return TABLE (traffic_system_id INT, billing_system_id INT)
	AS 
	BEGIN
		declare 
			@codeMonth as varchar(15), 
			@dateAsOf as datetime;

		select
			@dateAsOf = cast(media_month.month as varchar) + '/15/' + cast(media_month.year as varchar),
			@codeMonth = media_month.media_month
		from
			media_months media_month
		where
			@effective_date BETWEEN media_month.start_date AND media_month.end_date;

		with
		traffic_invoice_zone_map(
			traffic_zone_id,
			invoice_zone_id,
			type
		) as (
			select
				zone.zone_id traffic_zone_id,
				zone.zone_id invoice_zone_id,
				'Direct' type
			from
				udf_GetZonesAsOf(@dateAsOf) zone
			where
				zone.traffic = 1

			union all

			select	
				zone_group_zones.traffic_zone_id traffic_zone_id,
				zone_group_zones.invoice_zone_id invoice_zone_id,
				type
			from
				udf_ExpandZoneGroupsAsOf(@dateAsOf) zone_group_zones

			union all

			select
				interconnect_zone_map.traffic_zone_id,
				interconnect_zone_map.invoice_zone_id,
				interconnect_zone_map.type
			from
				udf_ExpandInterconnectsAsOf(@dateAsOf) interconnect_zone_map

			union all

			select
				turnkey_zone_map.traffic_zone_id,
				turnkey_zone_map.invoice_zone_id,
				turnkey_zone_map.type
			from
				udf_ExpandTurnkeysAsOf(@dateAsOf) turnkey_zone_map
			
			UNION ALL
			
			SELECT
				sz.zone_id,
				3747,
				'Direct'
			FROM
				uvw_systemzone_universe sz
			WHERE
				sz.system_id=67
				AND sz.start_date<=@dateAsOf AND (sz.end_date>=@dateAsOf OR sz.end_date IS NULL)
			
			UNION ALL
			
			SELECT
				sz.zone_id,
				1421,
				'Direct'
			FROM
				uvw_systemzone_universe sz
			WHERE
				sz.system_id=67
				AND sz.start_date<=@dateAsOf AND (sz.end_date>=@dateAsOf OR sz.end_date IS NULL)
		)
		
		INSERT INTO @return
			select distinct
				szt.system_id 'traffic_system_id',
				szb.system_id 'billing_system_id'
			from
				traffic_invoice_zone_map
				join media_months media_month (nolock) on
					@codeMonth = media_month.media_month
				join traffic_orders traffic_order (nolock) on
					traffic_order.start_date < media_month.end_date
					and
					media_month.start_date < traffic_order.end_date
					and
					traffic_invoice_zone_map.traffic_zone_id = traffic_order.zone_id
				join uvw_systemzone_universe szt (nolock) on szt.zone_id=traffic_invoice_zone_map.traffic_zone_id and szt.type='traffic' and (szt.start_date<=@effective_date AND (szt.end_date>=@effective_date OR szt.end_date IS NULL))
				join uvw_systemzone_universe szb (nolock) on szb.zone_id=traffic_invoice_zone_map.invoice_zone_id and szb.type='billing' and (szb.start_date<=@effective_date AND (szb.end_date>=@effective_date OR szb.end_date IS NULL))
	RETURN;
	END
