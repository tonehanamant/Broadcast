
CREATE PROCEDURE [dbo].[usp_REC_GetSystemsTraffickedDataByMonth]
	@media_month_id int,
	@system_id int
AS

select systems.code, systems.id, traffic_details.spot_length_id, sum(traffic_orders.ordered_spot_rate * traffic_orders.ordered_spots )
	from traffic_orders (NOLOCK)
	join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
	join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	join dbo.GetAllTrafficSystemsForAssociatedBillingSystem(@system_id) allsystems on allsystems.system_id = traffic_orders.system_id 
	join systems (NOLOCK) on systems.id = @system_id
	join media_months (NOLOCK)
		on traffic_orders.start_date >= media_months.start_date and
		traffic_orders.end_date <= media_months.end_date
where 
	media_months.id = @media_month_id 
	and traffic_details.network_id is not null and 
	 traffic_orders.release_id is not null and
	traffic_details.spot_length_id is not null 
	and traffic_detail_weeks.suspended = 0
	and traffic_orders.on_financial_reports = 1
	group by systems.code, systems.id, traffic_details.spot_length_id
