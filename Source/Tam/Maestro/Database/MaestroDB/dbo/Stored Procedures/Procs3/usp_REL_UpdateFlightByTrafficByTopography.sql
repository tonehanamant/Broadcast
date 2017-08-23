CREATE PROCEDURE [dbo].[usp_REL_UpdateFlightByTrafficByTopography]
(
	@traffic_detail_id int,
	@topography_id int,
	@daypart_id int,
	@new_start_date datetime,
	@new_end_date datetime,
	@start_date datetime,
	@end_date datetime
)

AS

BEGIN 

declare @traffic_id int;
declare @mydate datetime;
select 
    @mydate = traffic.start_date, @traffic_id = traffic.id 
from 
    traffic WITH (NOLOCK) 
    join traffic_details WITH (NOLOCK) 
        on traffic.id = traffic_details.traffic_id 
    where traffic_details.id =  @traffic_detail_id;

update 
	traffic_orders 
set 
	traffic_orders.start_date = @new_start_date, 
	traffic_orders.end_date = @new_end_date
from
	traffic_orders WITH (NOLOCK)
	join dbo.GetSystemsByTopographyAndDate(@topography_id, @mydate) sbtd on sbtd.system_id = traffic_orders.system_id 
where 
	traffic_orders.start_date = @start_date 
	and traffic_orders.end_date = @end_date 
	and traffic_orders.daypart_id = @daypart_id
	and traffic_orders.traffic_detail_id = @traffic_detail_id;
	
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
