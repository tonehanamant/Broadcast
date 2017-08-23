
CREATE Procedure [usp_TCS_GetTrafficRateCardMapsForTraffic]
(
	@traffic_id int
)
AS
select distinct
	trcm.*
from
	traffic_topography_rate_card_map trcm WITH (NOLOCK) 
WHERE
	trcm.traffic_id = @traffic_id
order by
	trcm.topography_id
