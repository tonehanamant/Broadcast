
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns dma_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetDMAsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[id] as [dma_id], 
		[code], 
		[name], 
		[rank],  
		[tv_hh],  
		[cable_hh],  
		[flag], 
		@dateAsOf as [as_of_date]
	from
		dmas (nolock)
	where
		@dateAsOf >= dmas.effective_date
		and
		1 = dmas.active
	union all
	select
		[dma_id], 
		[code], 
		[name], 
		[rank],  
		[tv_hh],  
		[cable_hh],  
		[flag], 
		@dateAsOf as [as_of_date]
	from
		dma_histories (nolock)
	where
		@dateAsOf between dma_histories.start_date and dma_histories.end_date
		and
		1 = dma_histories.active
);

