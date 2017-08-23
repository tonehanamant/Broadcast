
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetBillingSystemZonesAsOf]
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
		@dateAsOf as [as_of_date]
	from
		udf_GetSystemZonesAsOf(@dateAsOf) system_zones
	where
		'BILLING' = system_zones.type
);
