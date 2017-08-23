
CREATE PROCEDURE [dbo].[usp_REL_PurgeTrafficInRelease]
	@traffic_id int
AS
BEGIN
	delete traffic_orders 
	from traffic_orders ord
			join traffic_details td (NOLOCK) on ord.traffic_detail_id = td.id
	where td.traffic_id = @traffic_id 

	update traffic_detail_weeks 
		set suspended = 0 
	from traffic_detail_weeks (NOLOCK) 
		 join traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id 
	where traffic_details.traffic_id = @traffic_id;
END;
