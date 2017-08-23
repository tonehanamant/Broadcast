
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemZonesAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
	select
		[zone_id], 
		[system_id], 
		[type], 
		@dateAsOf as [as_of_date]
	from
		system_zones (nolock)
	where
		@dateAsOf >= system_zones.effective_date

	union

	select
		[zone_id], 
		[system_id], 
		[type], 
		@dateAsOf as [as_of_date]
	from
		system_zone_histories (nolock)
	where
		@dateAsOf between system_zone_histories.start_date and system_zone_histories.end_date
);
