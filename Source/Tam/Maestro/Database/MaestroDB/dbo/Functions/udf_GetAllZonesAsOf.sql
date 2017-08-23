
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetAllZonesAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[id] as [zone_id], 
		[code], 
		[name], 
		[type], 
		[primary], 
		[traffic], 
		[dma], 
		[flag], 
		[active],
		@dateAsOf as [as_of_date]
	from
		zones (nolock)
	where
		@dateAsOf >= zones.effective_date
	union all
	select
		[zone_id], 
		[code], 
		[name], 
		[type], 
		[primary], 
		[traffic], 
		[dma], 
		[flag], 
		[active],
		@dateAsOf as [as_of_date]
	from
		zone_histories (nolock)
	where
		@dateAsOf between zone_histories.start_date and zone_histories.end_date
);

