CREATE PROCEDURE [dbo].[usp_REL_GetTrafficStartAndEndDatesForSBTinRelease]
	@traffic_id INT,
	@system_id INT
WITH RECOMPILE
AS
BEGIN
	SELECT DISTINCT
		traffic_orders.start_date,
		traffic_orders.end_date
	FROM
		traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
		join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id 
			and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_detail_weeks.end_date 
	WHERE
		traffic_orders.system_id = @system_id
		and traffic_details.traffic_id = @traffic_id
		and traffic_detail_weeks.suspended = 0
		and traffic_orders.ordered_spots > 0
	ORDER BY
		traffic_orders.start_date;
END
