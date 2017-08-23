
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns active network_map records as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetNetworkMapsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[id] as [network_map_id], 
		[network_id], 
		[map_set], 
		[map_value],
		[flag], 
		@dateAsOf as [as_of_date]
	from
		network_maps (nolock)
	where
		@dateAsOf >= network_maps.effective_date
		and
		1 = network_maps.active
	union all
	select
		[network_map_id], 
		[network_id], 
		[map_set], 
		[map_value],
		[flag], 
		@dateAsOf as [as_of_date]
	from
		network_map_histories (nolock)
	where
		@dateAsOf between network_map_histories.start_date and network_map_histories.end_date
		and
		1 = network_map_histories.active
);

