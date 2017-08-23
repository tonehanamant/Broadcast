CREATE Procedure [dbo].[usp_TCS_GetDistinctNetworksAndDayparts]

      (

            @id Int

      )

AS

select 
	distinct networks.code, 
	traffic_details.network_id, 
	traffic_details.daypart_id 
from traffic_details (NOLOCK)
	join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
	join uvw_network_universe (NOLOCK) networks on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
where 
	traffic_details.traffic_id = @id 
order by 
	traffic_details.network_id, traffic_details.daypart_id

