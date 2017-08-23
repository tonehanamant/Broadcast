-- =============================================
-- Author:        David Sisson
-- Create date: 03/16/2009
-- Description:   Returns active network records as of specified date pulling 
--                      from both the networks and network_histories tables.
--                      If @dateAsOf is NULL, returns active records from the networks 
--                      table only.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetNetworksAsOf]
(     
		@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
		select
			[id] as [network_id], 
			[code], 
			[name], 
			[flag], 
			@dateAsOf as [as_of_date],
			language_id,
			affiliated_network_id,
			network_type_id
		from
			networks (nolock)
		where
			(
					@dateAsOf >= networks.effective_date
					or
					@dateAsOf is null
			)
			and
			1 = networks.active
		union all
		select
			[network_id], 
			[code], 
			[name], 
			[flag], 
			@dateAsOf as [as_of_date],
			language_id,
			affiliated_network_id,
			network_type_id
		from
			network_histories (nolock)
		where
			@dateAsOf between network_histories.start_date and network_histories.end_date
			and
			1 = network_histories.active
);
