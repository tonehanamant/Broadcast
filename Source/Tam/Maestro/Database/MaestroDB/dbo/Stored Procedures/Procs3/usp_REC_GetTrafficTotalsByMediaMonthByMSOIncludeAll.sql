
CREATE Procedure [dbo].[usp_REC_GetTrafficTotalsByMediaMonthByMSOIncludeAll]
(
	@media_month_id int,
	@business_id int
)

AS

DECLARE @effective_date DATETIME;
SET @effective_date = (select media_months.end_date from media_months (NOLOCK) where media_months.id = @media_month_id); 

	select distinct traffic.id, case when traffic.display_name is null or traffic.display_name = '''' then traffic.name else traffic.display_name end,
    sum(traffic_orders.ordered_spots * traffic_orders.ordered_spot_rate)
		from traffic_details (NOLOCK)
		join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
		join traffic_orders (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id 
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
		join media_months (NOLOCK) on 1=1
	where media_months.id = @media_month_id and traffic_orders.release_id is not null 
	and 
	(traffic.start_date >= media_months.start_date and traffic.start_date <= media_months.end_date or
	traffic.end_date >= media_months.start_date and traffic.end_date <= media_months.end_date)
	and traffic_orders.system_id in
	(
	select system_id from GetAllSystemsAssociatedToMSO(@business_id, @effective_date)
    )
	and traffic_detail_weeks.suspended = 0
	and traffic_orders.on_financial_reports = 1
	group by traffic.id, case when traffic.display_name is null or traffic.display_name = '''' then traffic.name else traffic.display_name end
	order by case when traffic.display_name is null or traffic.display_name = '''' then traffic.name else traffic.display_name end

