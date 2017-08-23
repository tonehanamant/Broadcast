
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetZoneBusinessesAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
	select
		[zone_id], 
		[business_id], 
		[type], 
		@dateAsOf as [as_of_date]
	from
		zone_businesses (nolock)
	where
		@dateAsOf >= zone_businesses.effective_date

	union

	select
		[zone_id], 
		[business_id], 
		[type], 
		@dateAsOf as [as_of_date]
	from
		zone_business_histories (nolock)
	where
		@dateAsOf between zone_business_histories.start_date and zone_business_histories.end_date
);
