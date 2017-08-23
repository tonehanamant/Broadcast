
CREATE PROCEDURE [dbo].[usp_REC_GetSystemsTraffickedDataByYear]
	@year int,
	@system_id int
AS

declare @start_date datetime;
declare @end_date datetime;

set @start_date = (select min(media_months.start_date) from media_months where media_months.year = @year);
set @end_date = (select max(media_months.end_date) from media_months where media_months.year = @year);

select systems.code, systems.id, traffic_details.spot_length_id, sum(traffic_orders.ordered_spot_rate * traffic_orders.ordered_spots )
	from traffic_orders (NOLOCK)
	join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
	join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	join dbo.GetAllTrafficSystemsForAssociatedBillingSystem(@system_id) allsystems on allsystems.system_id = traffic_orders.system_id 
	join systems (NOLOCK) on systems.id = @system_id
where 
	traffic_orders.start_date >= @start_date and traffic_orders.end_date <=  @end_date and 
	traffic_details.network_id is not null and traffic_orders.release_id is not null and
	traffic_details.spot_length_id is not null 
	and traffic_detail_weeks.suspended = 0
	and traffic_orders.on_financial_reports = 1
	group by systems.code, systems.id, traffic_details.spot_length_id
