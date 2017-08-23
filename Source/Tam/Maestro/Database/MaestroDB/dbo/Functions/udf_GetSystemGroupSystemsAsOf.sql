
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns system_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemGroupSystemsAsOf]
(	
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
	select
		[system_id], 
		[system_group_id], 
		@dateAsOf as [as_of_date]
	from
		system_group_systems (nolock)
	where
		@dateAsOf >= system_group_systems.effective_date

	union

	select
		[system_id], 
		[system_group_id], 
		@dateAsOf as [as_of_date]
	from
		system_group_system_histories (nolock)
	where
		@dateAsOf between system_group_system_histories.start_date and system_group_system_histories.end_date
);
