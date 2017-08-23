CREATE PROCEDURE [dbo].[usp_REL_GetRollupProposalDetails]
	@proposal_id INT
AS

DECLARE @audience_id_1 INT
DECLARE @audience_id_2 INT

SET @audience_id_1 = (SELECT audience_id FROM proposal_audiences WHERE proposal_id=@proposal_id AND ordinal=0)
SET @audience_id_2 = (SELECT audience_id FROM proposal_audiences WHERE proposal_id=@proposal_id AND ordinal=1)

SELECT 
	proposal_details.id, 
	MAX(network_maps.map_value), 
	spot_lengths.length, 
	proposal_details.daypart_id, 
	proposal_details.proposal_rate,
	proposal_details.start_date,
	proposal_details.end_date,
	proposal_details.num_spots,
	proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor 'coverage_universe',
	proposal_detail_audiences.rating,
	proposal_detail_audiences.us_universe,
	proposal_details.universal_scaling_factor
FROM 
	proposal_detail_audiences (NOLOCK)
	join proposal_details (NOLOCK) on proposal_details.id = proposal_detail_audiences.proposal_detail_id
	join proposals (NOLOCK) on proposals.id = proposal_details.proposal_id
	join uvw_network_universe (NOLOCK) networks on networks.network_id = proposal_details.network_id AND (networks.start_date<=proposals.start_date AND (networks.end_date>=proposals.start_date OR networks.end_date IS NULL))
	join uvw_networkmap_universe (NOLOCK) network_maps on proposal_details.network_id = network_maps.network_id AND (network_maps.start_date<=proposals.start_date AND (network_maps.end_date>=proposals.start_date OR network_maps.end_date IS NULL))
	join spot_lengths (NOLOCK) on spot_lengths.id = proposal_details.spot_length_id
WHERE
	proposal_details.proposal_id = @proposal_id 
	and network_maps.map_set = 'conimport'
GROUP BY 
	proposal_details.id, 
	spot_lengths.length, 
	proposal_details.daypart_id,
	proposal_details.proposal_rate,
	proposal_details.start_date,
	proposal_details.end_date,
	proposal_details.num_spots,
	proposal_detail_audiences.us_universe * proposal_details.universal_scaling_factor,
	proposal_detail_audiences.rating, 
	proposal_detail_audiences.us_universe, 
	proposal_details.universal_scaling_factor,
	proposal_details.network_id
ORDER BY 
	proposal_details.network_id

