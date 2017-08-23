CREATE PROCEDURE [dbo].[usp_REL_RemoveDetailLineFromOrder]
	@daypart_id int,
	@traffic_detail_id int,
	@start_date datetime,
	@end_date datetime,
	@topography_id int
AS
BEGIN
	declare @mydate datetime;
	select 
		@mydate = traffic.start_date 
	from 
		traffic (NOLOCK) 
		join traffic_details (NOLOCK) on traffic.id = traffic_details.traffic_id 
	where 
		traffic_details.id =  @traffic_detail_id;

	delete 
		t 
	from 
		traffic_orders t WITH (NOLOCK)
		join dbo.GetSystemsByTopographyAndDate(@topography_id, @mydate) sbtd on sbtd.system_id = t.system_id 
	where 
		t.traffic_detail_id = @traffic_detail_id 
		and t.start_date >= @start_date 
		and t.end_date <= @end_date
		and t.daypart_id = @daypart_id;

	update 
		traffic_detail_topographies
	set 
		traffic_detail_topographies.spots = 0
	where 
		traffic_detail_topographies.daypart_id = @daypart_id 
		and traffic_detail_topographies.topography_id = @topography_id
		and traffic_detail_topographies.traffic_detail_week_id in (
			select 
				id 
			from 
				traffic_detail_weeks (NOLOCK) 
			where 
			traffic_detail_weeks.traffic_detail_id = @traffic_detail_id 
			and traffic_detail_weeks.start_date = @start_date 
			and traffic_detail_weeks.end_date = @end_date
		)
END
