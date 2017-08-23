
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns system_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSystemsByTopographiesAsOf]
(	
	@topographyIds as UniqueIdTable READONLY,
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select
		sg.topography_id,
		s.system_id, 
		s.code, 
		s.name, 
		s.location, 
		s.spot_yield_weight, 
		s.traffic_order_format, 
		s.flag, 
		s.as_of_date
	from
		dbo.udf_GetSystemGroupsByTopographiesAsOf(@topographyIds, @dateAsOf) sg
		join dbo.udf_GetSystemGroupSystemsAsOf(@dateAsOf) sgs on
			sg.system_group_id = sgs.system_group_id
		join dbo.udf_GetSystemsAsOf(@dateAsOf) s on
			s.system_id = sgs.system_id

	union 

	select
		topography_id = ti.id,
		s.system_id, 
		s.code, 
		s.name, 
		s.location, 
		s.spot_yield_weight, 
		s.traffic_order_format, 
		s.flag, 
		s.as_of_date
	from
		dbo.udf_GetSystemsAsOf(@dateAsOf) s
		join topography_systems (nolock) ts on
			s.system_id = ts.system_id
		join @topographyIds ti 
			on ts.topography_id = ti.id
	where
		ts.include = 1

	except

	select
		topography_id = ti.id,
		s.system_id, 
		s.code, 
		s.name, 
		s.location, 
		s.spot_yield_weight, 
		s.traffic_order_format, 
		s.flag, 
		s.as_of_date
	from
		dbo.udf_GetSystemsAsOf(@dateAsOf) s
		join topography_systems (nolock) ts on
			s.system_id = ts.system_id
		join @topographyIds ti 
			on ts.topography_id = ti.id
	where
		ts.include = 0
);
