
CREATE Procedure [dbo].[usp_REC_GetMSOLostOpportunityForMonthTrafficDollars]
(
	@media_month_id int,
	@business_id int
)

AS

DECLARE @effective_date DATETIME;
SET @effective_date = (select media_months.end_date from media_months where id = @media_month_id);

	select sum(traffic_orders.ordered_spot_rate * traffic_orders.ordered_spots )
		from traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
		join dbo.GetAllBillingSystemsForBusiness(@business_id, @effective_date) bill_systems on 1=1 
		join systems (NOLOCK) on systems.id = bill_systems.system_id
		join dbo.GetAllTrafficAndBillingSystems() allsystems on allsystems.traffic_system_id = traffic_orders.system_id and bill_systems.system_id = allsystems.billing_system_id 
		join media_months (NOLOCK) 
			on traffic_orders.start_date between media_months.start_date and media_months.end_date
			and traffic_orders.end_date between media_months.start_date and media_months.end_date
	where 
		media_months.id = @media_month_id
		and traffic_details.network_id is not null and traffic_orders.release_id is not null and 
		traffic_orders.zone_id is not null and
		traffic_details.spot_length_id is not null 
		and  traffic_detail_weeks.suspended = 0
		and traffic_orders.on_financial_reports = 1
