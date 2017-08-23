
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns nielsen_network_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetNielsenNetworksAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[id] [nielsen_network_id], 
		[network_rating_category_id], 
		[nielsen_id], 
		[code], 
		[name], 
		[active], 
		@dateAsOf as [as_of_date]
	from
		nielsen_networks (nolock)
	where
		@dateAsOf >= nielsen_networks.effective_date
		and
		1 = nielsen_networks.active
	union all
	select
		[nielsen_network_id], 
		[network_rating_category_id], 
		[nielsen_id], 
		[code], 
		[name], 
		[active], 
		@dateAsOf as [as_of_date]
	from
		nielsen_network_histories (nolock)
	where
		@dateAsOf between nielsen_network_histories.start_date and nielsen_network_histories.end_date
		and
		1 = nielsen_network_histories.active
);
