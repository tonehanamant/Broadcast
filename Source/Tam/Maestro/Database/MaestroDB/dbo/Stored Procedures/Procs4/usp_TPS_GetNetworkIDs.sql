
CREATE PROCEDURE [dbo].[usp_TPS_GetNetworkIDs]
(
	@tid as int
)

AS

SELECT 
	distinct network_id 
from 
	traffic_details (NOLOCK)
where 
	traffic_id = @tid 
ORDER BY 
	network_id

