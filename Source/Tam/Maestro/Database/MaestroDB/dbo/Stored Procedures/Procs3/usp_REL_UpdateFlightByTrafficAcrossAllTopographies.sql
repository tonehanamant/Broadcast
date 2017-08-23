CREATE PROCEDURE [dbo].[usp_REL_UpdateFlightByTrafficAcrossAllTopographies]
(
	@traffic_detail_id int,
	@daypart_id int,
	@new_start_date datetime,
	@new_end_date datetime,
	@start_date datetime,
	@end_date datetime
)

AS

BEGIN 

declare @traffic_id int;
SELECT @traffic_id = traffic_id from traffic_details WITH (NOLOCK) WHERE traffic_details.id = @traffic_detail_id;

update 
	traffic_orders 
set 
	traffic_orders.start_date = @new_start_date, 
	traffic_orders.end_date = @new_end_date
where 
	traffic_orders.start_date = @start_date 
	and traffic_orders.end_date = @end_date 
	and traffic_orders.daypart_id = @daypart_id
	and traffic_orders.traffic_detail_id = @traffic_detail_id;

update 
	traffic_detail_weeks 
set 
	traffic_detail_weeks.start_date = @new_start_date, 
	traffic_detail_weeks.end_date = @new_end_date
from 
	traffic_detail_weeks  WITH (NOLOCK)
join
	traffic_detail_topographies  WITH (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
join
	traffic_details  WITH (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
where 
	traffic_detail_weeks.start_date = @start_date 
	and	traffic_detail_weeks.end_date = @end_date 
	and traffic_details.id = @traffic_detail_id;

	
update 
	traffic 
set 
	traffic.start_date = 
	(select min(traffic_detail_weeks.start_date) 
		from 
			traffic_details WITH (NOLOCK)
		join 
			traffic_detail_weeks WITH (NOLOCK)
			on traffic_details.id = traffic_detail_weeks.traffic_detail_id 
		where  
			traffic_details.traffic_id = @traffic_id
	),
	traffic.end_date = 
	(select max(traffic_detail_weeks.end_date) 
		from 
			traffic_details WITH (NOLOCK)
		join 
			traffic_detail_weeks WITH (NOLOCK)
			on traffic_details.id = traffic_detail_weeks.traffic_detail_id 
		where  
			traffic_details.traffic_id = @traffic_id
	)
where 
	traffic.id = @traffic_id ;
END
