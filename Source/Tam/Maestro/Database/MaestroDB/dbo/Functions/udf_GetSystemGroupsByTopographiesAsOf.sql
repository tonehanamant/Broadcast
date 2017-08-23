

-- =============================================
-- Author:		John Carsley
-- Create date: 02/07/2013
-- Description:	Returns system_group_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemGroupsByTopographiesAsOf]
(	
	@topographyIds as UniqueIdTable READONLY,
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	
	select
		topography_id = ti.id, 
		sg.system_group_id,
		sg.name,
		sg.flag,
		sg.as_of_date
	from
		dbo.udf_GetSystemGroupsAsOf(@dateAsOf) sg
		join topography_system_groups (nolock) tsg on
			sg.system_group_id = tsg.system_group_id
		join @topographyIds ti on tsg.topography_id = ti.id
	where
		tsg.include = 1

	except

	select
		topography_id = ti.id,
		sg.system_group_id,
		sg.name,
		sg.flag,
		sg.as_of_date
	from
		dbo.udf_GetSystemGroupsAsOf(@dateAsOf) sg
		join topography_system_groups (nolock) tsg on
			sg.system_group_id = tsg.system_group_id
		join @topographyIds ti on tsg.topography_id = ti.id
	where
		tsg.include = 0
);
