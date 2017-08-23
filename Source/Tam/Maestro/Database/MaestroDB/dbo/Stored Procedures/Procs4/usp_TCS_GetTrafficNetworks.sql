CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficNetworks]
(
		@traffic_id as int
)

AS

select distinct 
		networks.network_id, 
		networks.code,
		networks.name, 
		networks.active,
		networks.flag,
		networks.start_date,
		networks.language_id,
		networks.affiliated_network_id,
		networks.network_type_id
from 
		traffic_details (NOLOCK) 
		join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
		join uvw_network_universe (NOLOCK) networks 
					on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
where
		traffic_details.traffic_id = @traffic_id
ORDER BY 
		networks.code
