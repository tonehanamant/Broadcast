

 

CREATE  Procedure [dbo].[usp_TCS_GetTrafficRatesByTrafficID]

      (

            @traffic_id Int

      )

 

AS

 

select id, traffic_detail_id, proposal_detail_id, proposal_rate, proposal_spots

 from traffic_details_proposal_details_map (NOLOCK) where traffic_detail_id in 

(select id from traffic_details where traffic_id = @traffic_id)


