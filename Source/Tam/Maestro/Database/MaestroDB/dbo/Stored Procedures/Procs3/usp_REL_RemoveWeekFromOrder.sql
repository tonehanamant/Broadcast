CREATE Procedure [dbo].[usp_REL_RemoveWeekFromOrder]
(
                @traffic_id int,
                @start_date datetime,
                @end_date datetime
)
AS
BEGIN

	delete t from 
       traffic_orders t
	   join traffic_details WITH (NOLOCK) on traffic_details.id = t.traffic_detail_id
       where traffic_details.traffic_id = @traffic_id 
           and t.start_date >= @start_date 
           and t.end_date <= @end_date

	delete t from 
       traffic_detail_topographies t
	   join traffic_detail_weeks WITH (NOLOCK) on traffic_detail_weeks.id = t.traffic_detail_week_id
	   join traffic_details WITH (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
       where traffic_details.traffic_id = @traffic_id 
           and traffic_detail_weeks.start_date >= @start_date 
           and traffic_detail_weeks.end_date <= @end_date
		   
	delete t from 
       traffic_detail_weeks t
	   join traffic_details WITH (NOLOCK) on traffic_details.id = t.traffic_detail_id
       where traffic_details.traffic_id = @traffic_id 
           and t.start_date >= @start_date 
           and t.end_date <= @end_date 

END

