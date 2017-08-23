
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficStartAndEndDatesForTrafficinRelease]
(
      @release_id int,
      @traffic_id int
)
AS

select 
      distinct 
        traffic_orders.start_date,
      traffic_orders.end_date
from
      traffic_orders (NOLOCK)
      join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
        join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
WHERE
      traffic_details.traffic_id = @traffic_id
      and
      traffic_orders.release_id = @release_id
        and traffic_orders.system_id not in (668, 667)
        and
      traffic_orders.active = 1
      and 
      (
            traffic_detail_weeks.suspended = 0
      )
order by
      traffic_orders.start_date;

