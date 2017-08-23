
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns active network_substitution records as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetNetworkSubstitutionsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select 
		[network_id], 
		[substitution_category_id], 
		[substitute_network_id],
		[weight], 
		@dateAsOf as [as_of_date]
	from
		network_substitutions (nolock)
	where
		@dateAsOf >= network_substitutions.effective_date
	union all
	select
		[network_id], 
		[substitution_category_id], 
		[substitute_network_id],
		[weight], 
		@dateAsOf as [as_of_date]
	from
		network_substitution_histories (nolock)
	where
		@dateAsOf between network_substitution_histories.start_date and network_substitution_histories.end_date
);

