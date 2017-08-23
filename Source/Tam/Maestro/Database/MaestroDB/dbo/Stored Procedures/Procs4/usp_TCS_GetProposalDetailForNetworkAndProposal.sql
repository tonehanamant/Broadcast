

CREATE PROCEDURE [dbo].[usp_TCS_GetProposalDetailForNetworkAndProposal]
(
            @proposal_id as int,
            @network_id as int
)

AS

select proposal_id, proposal_rate, num_spots 
	from proposal_details  (NOLOCK)
where network_id = @network_id 
	and proposal_id = @proposal_id

