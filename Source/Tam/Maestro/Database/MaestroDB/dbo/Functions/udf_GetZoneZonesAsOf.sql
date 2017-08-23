	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 02/13/2015
	-- Description:	
	-- =============================================
	CREATE FUNCTION [dbo].[udf_GetZoneZonesAsOf]
	(
		@dateAsOf as datetime,
		@type as varchar(63)
	)
	RETURNS TABLE
	AS
	RETURN
	(
		select
			[primary_zone_id], 
			[secondary_zone_id], 
			[type], 
			@dateAsOf as [as_of_date]
		from
			zone_zones (nolock)
		where
			@dateAsOf >= zone_zones.effective_date
			AND @type = zone_zones.[type]

		union

		select
			[primary_zone_id], 
			[secondary_zone_id], 
			[type], 
			@dateAsOf as [as_of_date]
		from
			zone_zone_histories (nolock)
		where
			@dateAsOf between zone_zone_histories.start_date and zone_zone_histories.end_date
			AND @type = zone_zone_histories.[type]
	);
