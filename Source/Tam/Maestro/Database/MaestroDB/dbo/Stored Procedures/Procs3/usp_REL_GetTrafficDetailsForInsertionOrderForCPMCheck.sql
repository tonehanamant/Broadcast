CREATE PROCEDURE [dbo].[usp_REL_GetTrafficDetailsForInsertionOrderForCPMCheck]
      @traffic_id as int
AS

SELECT 
	all_totals.traffic_detail_id, 
	networks.code, 
	spot_lengths.length, 
	all_totals.daypart_id, 
	traffic.start_date,
	traffic.end_date,
	cast(case when all_totals.ordered_spot_rate > 0 then all_totals.total_dollars / all_totals.ordered_spot_rate else 0.00 end as float),
	(select count(*) from traffic_flights where traffic_id = @traffic_id and selected = 1),
	cast(all_totals.ordered_spots as int),
	all_totals.ordered_spot_rate,
	cast(0.00 as float) ,
	all_totals.total_dollars,
	traffic_detail_audiences.traffic_rating,
	cast(all_totals.subscribers as int)
from 
	traffic (NOLOCK) 
	join traffic_details (NOLOCK) on traffic.id = traffic_details.traffic_id
	join traffic_detail_audiences (NOLOCK) on traffic_details.id = traffic_detail_audiences.traffic_detail_id and traffic.audience_id = traffic_detail_audiences.audience_id
	join GetTrafficReleaseTotalByNetworkExcludeDish(@traffic_id) all_totals 
		on all_totals.traffic_detail_id = traffic_details.id 
	join uvw_network_universe (NOLOCK) networks on networks.network_id = traffic_details.network_id AND (networks.start_date<=traffic.start_date AND (networks.end_date>=traffic.start_date OR networks.end_date IS NULL))
	join spot_lengths (NOLOCK) on spot_lengths.id = traffic_details.spot_length_id
where
	traffic_details.traffic_id = @traffic_id 
ORDER BY 
	networks.code, traffic.start_date
