
CREATE Procedure [dbo].[usp_REC_GetTrafficTotalsByMediaMonthBySystem]
(
	@media_month_id int,
	@system_id int
)

AS

DECLARE @effective_date DATETIME;
SET @effective_date = getdate();

	select distinct traffic.id, case when traffic.display_name is null or traffic.display_name = '' then traffic.name else traffic.display_name end,
sum(traffic_orders.ordered_spots * traffic_orders.ordered_spot_rate)
		from traffic_details (NOLOCK)
		join traffic_orders (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id 
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
		join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id  
		join media_months (NOLOCK) 
			on traffic_orders.start_date >= media_months.start_date and traffic_orders.start_date <= media_months.end_date 
		join dbo.GetAllTrafficSystemsForAssociatedBillingSystem(@system_id) allsystems on allsystems.system_id = traffic_orders.system_id 
	where media_months.id = @media_month_id and traffic_orders.release_id is not null 
	and traffic_detail_weeks.suspended = 0
	and traffic_orders.on_financial_reports = 1
	group by traffic.id, case when traffic.display_name is null or traffic.display_name = '' then traffic.name else traffic.display_name end
	order by case when traffic.display_name is null or traffic.display_name = '' then traffic.name else traffic.display_name end
