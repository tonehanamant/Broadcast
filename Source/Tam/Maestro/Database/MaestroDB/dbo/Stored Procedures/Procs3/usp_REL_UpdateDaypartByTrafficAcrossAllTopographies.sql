CREATE PROCEDURE [dbo].[usp_REL_UpdateDaypartByTrafficAcrossAllTopographies]
	@traffic_detail_id int,
	@old_daypart_id int,
	@new_daypart_id int,
	@start_date datetime,
	@end_date datetime
AS
BEGIN
	declare @mydate datetime;
					
	update 
		traffic_orders 
	set 
		traffic_orders.daypart_id = @new_daypart_id 
	from 
		traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id 
	where 
		traffic_orders.traffic_detail_id = @traffic_detail_id
		and traffic_orders.daypart_id = @old_daypart_id
		and	(traffic_orders.start_date >= @start_date and traffic_orders.end_date <= @end_date)
		

	update 
		traffic_detail_topographies 
	set 
		traffic_detail_topographies.daypart_id = @new_daypart_id 
	from 
		traffic_detail_topographies (NOLOCK) 
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
		join traffic_details (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id  
	where
		(traffic_detail_weeks.start_date >= @start_date and traffic_detail_weeks.end_date <= @end_date)
		and traffic_details.id = @traffic_detail_id
		and traffic_detail_topographies.daypart_id = @old_daypart_id;
END
