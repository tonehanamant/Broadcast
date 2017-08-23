CREATE PROCEDURE [dbo].[usp_REL_GetAllSystemsForTopographiesInRelease]
	@topography_id int,
	@release_id int
AS

declare @effective_date datetime;
select @effective_date = min(start_date) from traffic_orders (NOLOCK) where release_id = @release_id and ordered_spots > 0;

select
	distinct sbt.system_id
from
	dbo.GetSystemsByTopographyAndDate(@topography_id, @effective_date) sbt
	join traffic_orders (NOLOCK) on traffic_orders.system_id = sbt.system_id 
where
	traffic_orders.release_id = @release_id
order by
	system_id
