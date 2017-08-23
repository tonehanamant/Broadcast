	-- =============================================
	-- Author:		David Sisson
	-- Create date: 9/22/2010
	-- Description:	<Description,,>
	-- =============================================
	-- usp_ACS_GetTrafficToBillingZoneAssociations 391
	CREATE PROCEDURE [dbo].[usp_ACS_GetTrafficToBillingZoneAssociations]
		@media_month_id INT
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
			media_month.id = @media_month_id;

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
		
		select distinct
			traffic_invoice_zone_map.traffic_zone_id,
			traffic_invoice_zone_map.invoice_zone_id,
			traffic_invoice_zone_map.type
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
	END
