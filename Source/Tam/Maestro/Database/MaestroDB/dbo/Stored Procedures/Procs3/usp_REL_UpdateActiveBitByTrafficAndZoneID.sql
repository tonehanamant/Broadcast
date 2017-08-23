
CREATE PROCEDURE [dbo].[usp_REL_UpdateActiveBitByTrafficAndZoneID]
      @traffic_id as int,
	  @zone_id as int,
	  @active bit
AS

	update traffic_orders set active = @active
	from 
		traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) 
			on traffic_details.id = traffic_orders.traffic_detail_id
	where 
		traffic_orders.zone_id = @zone_id 
		and traffic_details.traffic_id = @traffic_id
