


CREATE Procedure [dbo].[usp_REC_GetTrafficByMediaMonth]
(
	@media_month_id int
)

AS

DECLARE @effective_date DATETIME;
SET @effective_date = getdate();

	select distinct traffic.id, case when traffic.display_name is null or traffic.display_name = '' then traffic.name else traffic.display_name end
		from traffic (NOLOCK)
		join traffic_details (NOLOCK) on traffic_details.traffic_id = traffic.id
		join traffic_orders (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id 
		join media_months (NOLOCK) on 1=1 
		where media_months.id = @media_month_id and traffic_orders.release_id is not null and 
	(
	traffic.start_date >= media_months.start_date and traffic.start_date <= media_months.end_date or 
	traffic.end_date >= media_months.start_date and traffic.end_date <= media_months.end_date)
	and traffic_orders.on_financial_reports = 1
	order by case when traffic.display_name is null or traffic.display_name = '' then traffic.name else traffic.display_name end


