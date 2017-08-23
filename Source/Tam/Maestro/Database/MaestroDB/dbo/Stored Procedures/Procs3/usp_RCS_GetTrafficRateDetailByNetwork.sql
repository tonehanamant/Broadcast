CREATE PROCEDURE [dbo].[usp_RCS_GetTrafficRateDetailByNetwork]
(
	@traffic_rate_card_id int,
	@network_id int,
	@spot_length_id int
)

AS

select 
	td.daypart_id, tr.rate 
from 
	traffic_rate_card_details (NOLOCK) td
join 
	traffic_rate_card_detail_rates tr on td.id = tr.traffic_rate_card_detail_id and tr.spot_length_id = @spot_length_id
where 
	td.network_id = @network_id
	and td.traffic_rate_card_id = @traffic_rate_card_id

