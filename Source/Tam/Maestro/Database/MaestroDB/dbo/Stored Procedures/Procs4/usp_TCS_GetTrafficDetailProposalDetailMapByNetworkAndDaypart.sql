
 

 

CREATE Procedure [dbo].[usp_TCS_GetTrafficDetailProposalDetailMapByNetworkAndDaypart]

      (

            @traffic_id Int,

                        @network_id int, 

                        @daypart_id int

      )

AS

 

SELECT id, traffic_detail_id, proposal_detail_id, proposal_rate, proposal_spots from 

traffic_details_proposal_details_map (NOLOCK) where traffic_details_proposal_details_map.traffic_detail_id in 

(select id from traffic_details (NOLOCK) where traffic_id = @traffic_id and network_id = @network_id and daypart_id = @daypart_id)

