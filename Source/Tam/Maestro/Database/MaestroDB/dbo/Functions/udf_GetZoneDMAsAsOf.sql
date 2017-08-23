
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetZoneDMAsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
	select
		[zone_id], 
		[dma_id], 
		[weight], 
		@dateAsOf as [as_of_date]
	from
		zone_dmas (nolock)
	where
		@dateAsOf >= zone_dmas.effective_date

	union

	select
		[zone_id], 
		[dma_id], 
		[weight], 
		@dateAsOf as [as_of_date]
	from
		zone_dma_histories (nolock)
	where
		@dateAsOf between zone_dma_histories.start_date and zone_dma_histories.end_date
);
