
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetZonesAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[zone_id], 
		[code], 
		[name], 
		[type], 
		[primary], 
		[traffic], 
		[dma], 
		[flag], 
		@dateAsOf as [as_of_date]
	from
		udf_GetAllZonesAsOf(@dateAsOf) zones
	where
		1 = zones.active
);

