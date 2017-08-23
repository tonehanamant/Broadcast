CREATE PROCEDURE [dbo].[usp_REL_GetDistinctNetworksFromOrder]
		@traffic_id int,
		@system_id int
AS
declare @query as varchar(max);

set @query = '
select distinct networks.network_id, networks.code, networks.name, networks.active, networks.flag, networks.start_date, networks.language_id, networks.affiliated_network_id, networks.network_type_id 
		from traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
		join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
		join uvw_network_universe networks (NOLOCK) on networks.network_id = traffic_details.network_id  AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
	where traffic_details.traffic_id = ' + cast(@traffic_id as varchar(25));

if(@system_id is not null)
begin
		set @query = @query + ' and traffic_orders.system_id = ' + cast(@system_id as varchar(25)); 
end;

--print @query;

exec (@query);
