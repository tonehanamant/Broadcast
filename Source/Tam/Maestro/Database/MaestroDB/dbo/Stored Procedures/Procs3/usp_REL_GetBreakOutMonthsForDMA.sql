
-- Updated so that the query will not hit the traffic_orders table when traffic.plan_type is not 0
CREATE PROCEDURE [dbo].[usp_REL_GetBreakOutMonthsForDMA]
	@system_id int,
	@traffic_id int,
	@dma_id int
AS

select 
	media_months.year, 
	media_months.month,
	sum(traffic_orders.ordered_spot_rate * traffic_orders.ordered_spots), 
	sum(traffic_orders.ordered_spots)
from 
	traffic_orders  (NOLOCK)
	join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
	join traffic_detail_weeks (NOLOCK) 
		on traffic_detail_weeks.traffic_detail_id = traffic_details.id 
		and traffic_orders.start_date >= traffic_detail_weeks.start_date 
		and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	join traffic (NOLOCK) 
		on traffic.id = traffic_details.traffic_id
		and traffic.plan_type = 0
	join media_months (NOLOCK) 
		on traffic_orders.start_date >= media_months.start_date 
		and traffic_orders.end_date < =	media_months.end_date
where traffic.id = @traffic_id 
	and traffic_orders.system_id = @system_id 
	and traffic_orders.zone_id 
	in
	(
		select zone_id from uvw_zonedma_universe where dma_id = @dma_id and 
		(start_date<=getdate() AND (end_date>=getdate() OR end_date IS NULL))
	 )
	and traffic_detail_weeks.suspended = 0
	and traffic_orders.on_financial_reports = 1
	and traffic_orders.active = 1
group by media_months.year, media_months.month
