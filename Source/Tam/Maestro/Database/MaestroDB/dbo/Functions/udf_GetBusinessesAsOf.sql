
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns business_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetBusinessesAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[id] as [business_id], 
		[code], 
		[name], 
		[type], 
		@dateAsOf as [as_of_date]
	from
		businesses (nolock)
	where
		@dateAsOf >= businesses.effective_date
		and
		1 = businesses.active
	union all
	select
		[business_id], 
		[code], 
		[name], 
		[type], 
		@dateAsOf as [as_of_date]
	from
		business_histories (nolock)
	where
		@dateAsOf between business_histories.start_date and business_histories.end_date
		and
		1 = business_histories.active
);

