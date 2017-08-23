
CREATE PROCEDURE [dbo].[usp_TCS_GetProposalNetworksForBudget]
(
    @proposal_id as int
)
AS
BEGIN
	select 
		pd.network_id, 
		networks.code, 
		pd.proposal_rate, 
		pd.num_spots,
		pd.daypart_id, 
		pd.id,
		sl.length,
		nm.map_value
	from 
		proposal_details pd WITH (NOLOCK) 
		join proposals p WITH (NOLOCK) on p.id = pd.proposal_id
		join spot_lengths sl WITH (NOLOCK) on sl.id = pd.spot_length_id
		join uvw_network_universe networks WITH (NOLOCK) on networks.network_id = pd.network_id AND (networks.start_date<=p.start_date AND (networks.end_date>=p.start_date OR networks.end_date IS NULL))
		left join uvw_network_maps nm WITH (NOLOCK) on nm.network_id = networks.network_id and (networks.start_date<=p.start_date AND (networks.end_date>=p.start_date OR networks.end_date IS NULL)) and nm.map_set = 'Nielsen'
	where 
		pd.proposal_id = @proposal_id and pd.num_spots > 0
	order by 
		networks.code, pd.daypart_id
END
