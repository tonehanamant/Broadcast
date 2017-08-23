

CREATE Procedure [dbo].[usp_REL_GetTrafficSpotsForAllSystems]
	@traffic_id int,
	@topography_id int
AS

declare @iscarve_or_cox int;
set @iscarve_or_cox = 0;

select @iscarve_or_cox = 
	case 
		when map_value = 'crv' then 1 
		when map_value = 'cox' then 1 
		when map_value = 'dsh' then 1 
		else 0 
	end 
from 
	topography_maps WITH (NOLOCK)
where 
	map_set = 'release_bd_model' 
	and topography_id = @topography_id;

declare @start_date datetime;

select @start_date = min(start_date) from traffic_orders (NOLOCK)
join traffic_details WITH (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
where traffic_details.traffic_id = @traffic_id and traffic_orders.ordered_spots > 0
and traffic_orders.active = 1;

IF(@start_date IS NULL)
BEGIN 
	select @start_date = start_date from traffic WITH (NOLOCK) where id = @traffic_id;
END;

IF(@iscarve_or_cox = 0)
BEGIN
	select 
		distinct 
		networks.code, 
		traffic_details.network_id, 
		traffic_orders.daypart_id, 
		traffic_orders.traffic_detail_id,
		cast(traffic_detail_topographies.spots as float), 
		traffic_orders.start_date, 
		traffic_orders.end_date,
		count(distinct traffic_orders.system_id)
	from traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id 
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
		join traffic_detail_topographies (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id 
		join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
		join uvw_network_universe (NOLOCK) networks on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
		join GetSystemsByTopographyAndDate(@topography_id, @start_date) sbtd on traffic_orders.system_id = sbtd.system_id
	where 
		traffic_details.traffic_id = @traffic_id 
		and traffic_detail_topographies.topography_id = @topography_id 
		and traffic_detail_weeks.suspended = 0
		and traffic_orders.on_financial_reports = 1
		and traffic_orders.active = 1
	group by
		networks.code, 
		traffic_details.network_id, 
		traffic_orders.daypart_id, 
		traffic_orders.traffic_detail_id,
		cast(traffic_detail_topographies.spots as float), 
		traffic_orders.start_date, 
		traffic_orders.end_date
	order by 
		networks.code, 
		traffic_orders.start_date, 
		count(distinct traffic_orders.system_id) DESC,
		traffic_orders.daypart_id;
END
ELSE
BEGIN
	select 
		distinct 
		networks.code, 
		traffic_details.network_id, 
		traffic_orders.daypart_id, 
		traffic_orders.traffic_detail_id,
		cast(traffic_orders.ordered_spots as float), 
		traffic_orders.start_date, 
		traffic_orders.end_date,
		count(distinct traffic_orders.system_id)
	from traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id 
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
		join traffic_detail_topographies (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id 
		join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
		join uvw_network_universe (NOLOCK) networks on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
		join GetSystemsByTopographyAndDate(@topography_id, @start_date) sbtd on traffic_orders.system_id = sbtd.system_id
	where 
		traffic_details.traffic_id = @traffic_id 
		and traffic_detail_topographies.topography_id = @topography_id 
		and traffic_detail_weeks.suspended = 0
		and traffic_orders.on_financial_reports = 1
		and traffic_orders.active = 1
	group by
		networks.code, 
		traffic_details.network_id, 
		traffic_orders.daypart_id, 
		traffic_orders.traffic_detail_id,
		cast(traffic_orders.ordered_spots as float), 
		traffic_orders.start_date, 
		traffic_orders.end_date
	order by 
		networks.code, 
		traffic_orders.start_date, 
		count(distinct traffic_orders.system_id) DESC,
		traffic_orders.daypart_id
END
