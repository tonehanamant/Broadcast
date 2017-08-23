CREATE Procedure [dbo].[usp_REL_RemoveDetailLineFromOrderAcrossAllTopographies]
(
                @traffic_detail_id int,
				@daypart_id int,
                @start_date datetime,
                @end_date datetime
)
AS
BEGIN

declare @num_lines_left int;

BEGIN TRANSACTION;

delete t from 
    traffic_orders t
    where t.traffic_detail_id = @traffic_detail_id 
	and t.daypart_id = @daypart_id
    and t.start_date >= @start_date 
    and t.end_date <= @end_date
COMMIT TRANSACTION;

select @num_lines_left = count(*) from traffic_orders t WITH (NOLOCK)
where t.traffic_detail_id = @traffic_detail_id 
	and t.start_date >= @start_date 
    and t.end_date <= @end_date
	
	IF(@num_lines_left = 0)
	BEGIN
		update traffic_detail_topographies
			set traffic_detail_topographies.spots = 0
		from traffic_detail_topographies WITH (NOLOCK)
		JOIN traffic_detail_weeks WITH (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
		where 
			traffic_detail_weeks.traffic_detail_id = @traffic_detail_id 
			and traffic_detail_weeks.start_date <= @start_date 
			and traffic_detail_weeks.end_date >= @end_date
	END

END
