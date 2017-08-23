

CREATE Procedure [dbo].[usp_REL_RemoveSystemFromTrafficInRelease]
(
	@traffic_id int,
	@system_id int
)
AS

delete 
	from 
		traffic_orders 
	where system_id = @system_id and traffic_detail_id in (select id from traffic_details (NOLOCK) where traffic_id = @traffic_id)


