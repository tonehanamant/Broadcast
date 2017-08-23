-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetZoneNetworksAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
	select
		[zone_id], 
		[network_id], 
		[source], 
		[trafficable], 
		[primary],
		[subscribers], 
		@dateAsOf as [as_of_date]
	from
		zone_networks (nolock)
	where
		@dateAsOf >= zone_networks.effective_date
		and
		0 < zone_networks.subscribers

	union

	select
		[zone_id], 
		[network_id], 
		[source], 
		[trafficable], 
		[primary],
		[subscribers], 
		@dateAsOf as [as_of_date]
	from
		zone_network_histories (nolock)
	where
		@dateAsOf between zone_network_histories.start_date and zone_network_histories.end_date
		and
		0 < zone_network_histories.subscribers
);
