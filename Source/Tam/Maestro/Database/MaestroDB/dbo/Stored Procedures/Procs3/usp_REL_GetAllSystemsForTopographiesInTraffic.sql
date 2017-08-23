CREATE PROCEDURE [dbo].[usp_REL_GetAllSystemsForTopographiesInTraffic]
	@topography_id int,
	@traffic_id int
AS

declare @effective_date datetime;
select @effective_date = min(start_date) from traffic_orders (NOLOCK) join traffic_details (NOLOCK) on
	traffic_orders.traffic_detail_id = traffic_details.id
where 
	traffic_details.traffic_id = @traffic_id and ordered_spots > 0;

select
	distinct sbt.system_id
from
	dbo.GetSystemsByTopographyAndDate(@topography_id, @effective_date) sbt
	join traffic_orders (NOLOCK) on traffic_orders.system_id = sbt.system_id 
	join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
where
	traffic_details.traffic_id = @traffic_id and traffic_orders.on_financial_reports = 1
order by
	system_id
