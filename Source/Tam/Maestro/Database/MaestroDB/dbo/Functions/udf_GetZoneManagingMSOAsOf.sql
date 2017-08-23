
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetZoneManagingMSOAsOf]
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
		@dateAsOf as [as_of_date]
	from
		udf_GetZoneBusinessesAsOf(@dateAsOf) zone_businesses
	where
		'MANAGEDBY' = zone_businesses.type
);
