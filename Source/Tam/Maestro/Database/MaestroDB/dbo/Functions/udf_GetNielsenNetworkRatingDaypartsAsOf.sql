
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns system_group_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetNielsenNetworkRatingDaypartsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		nielsen_network_id, 
		daypart_id, 
		@dateAsOf as [as_of_date]
	from
		nielsen_network_rating_dayparts with (nolock)
	where
		@dateAsOf >= nielsen_network_rating_dayparts.effective_date
	union all
	select
		nielsen_network_id, 
		daypart_id, 
		@dateAsOf as [as_of_date]
	from
		nielsen_network_rating_daypart_histories with (nolock)
	where
		@dateAsOf between nielsen_network_rating_daypart_histories.start_date and nielsen_network_rating_daypart_histories.end_date
);

