

 

 

CREATE Procedure [dbo].[usp_TCS_GetTrafficDetailProposalDetailMapsByTrafficID]

      (

            @traffic_id Int

       )

 

AS

 

SELECT id, traffic_detail_id, proposal_detail_id, proposal_rate, proposal_spots

from traffic_details_proposal_details_map (NOLOCK) where

traffic_detail_id in (select id from traffic_details (NOLOCK) where traffic_id = @traffic_id)


