
CREATE PROCEDURE [dbo].[usp_REL_GetSystemWeightingFactors]
	@topography_id int
AS
 
select 
	custom_traffic.zone_id, 
	custom_traffic.network_id, 
	custom_traffic.traffic_factor, 
	custom_traffic.effective_date
from
	custom_traffic (NOLOCK)
where
	custom_traffic.effective_date <= getdate()
order by
	custom_traffic.zone_id, 
	custom_traffic.network_id


