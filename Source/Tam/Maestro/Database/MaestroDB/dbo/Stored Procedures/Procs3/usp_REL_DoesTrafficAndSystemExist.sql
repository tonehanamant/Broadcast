CREATE Procedure [dbo].[usp_REL_DoesTrafficAndSystemExist]
      (
            @system_id Int,
			@traffic_id int
      )
AS

select traffic_orders.id from traffic_orders WITH (NOLOCK) JOIN traffic_details WITH (NOLOCK)
	on traffic_details.id = traffic_orders.traffic_detail_id 
	where 
		system_id = @system_id and traffic_details.traffic_id = @traffic_id
