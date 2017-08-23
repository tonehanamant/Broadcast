CREATE PROCEDURE [dbo].[usp_REL_GetTrafficOrdersForTopographyByDetailIDByDaypart]
	@traffic_detail_id int,
	@topography_id int,
	@start_date datetime,
	@end_date datetime
AS
	declare @start_date_order datetime;

	select @start_date_order = min(start_date) from traffic_orders (NOLOCK)
	where traffic_orders.traffic_detail_id = @traffic_detail_id and traffic_orders.ordered_spots > 0
	and traffic_orders.on_financial_reports = 1;

	IF(@start_date_order IS NULL)
	BEGIN 
		  select @start_date_order = start_date from traffic WITH (NOLOCK) join 
		  traffic_details WITH (NOLOCK) on traffic_details.traffic_id = traffic.id 
		  where traffic_details.id = @traffic_detail_id;
	END;

	select 
		traffic_orders.*
	from 
		traffic_orders (NOLOCK) 
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
		join GetSystemsByTopographyAndDate(@topography_id, @start_date_order) sbtd on traffic_orders.system_id = sbtd.system_id
	where 
		traffic_orders.traffic_detail_id = @traffic_detail_id  
		and traffic_orders.start_date >= @start_date
		and traffic_orders.end_date <= @end_date
