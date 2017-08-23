
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns system_group_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemGroupsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		[id] as [system_group_id], 
		[name], 
		[flag], 
		@dateAsOf as [as_of_date]
	from
		system_groups (nolock)
	where
		@dateAsOf >= system_groups.effective_date
		and
		1 = system_groups.active
	union all
	select
		[system_group_id], 
		[name], 
		[flag], 
		@dateAsOf as [as_of_date]
	from
		system_group_histories (nolock)
	where
		@dateAsOf between system_group_histories.start_date and system_group_histories.end_date
		and
		1 = system_group_histories.active
);

