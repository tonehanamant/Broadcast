
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns system_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[id] as [system_id], 
		[code], 
		[name], 
		[location], 
		[spot_yield_weight], 
		[traffic_order_format], 
		[flag], 
		@dateAsOf as [as_of_date]
	from
		systems (nolock)
	where
		@dateAsOf >= systems.effective_date
		and
		1 = systems.active
	union all
	select
		[system_id], 
		[code], 
		[name], 
		[location], 
		[spot_yield_weight], 
		[traffic_order_format], 
		[flag], 
		@dateAsOf as [as_of_date]
	from
		system_histories (nolock)
	where
		@dateAsOf between system_histories.start_date and system_histories.end_date
		and
		1 = system_histories.active
);

