
Create Procedure [dbo].[REL_PurgeTrafficDetailLine]
      (
            @traffic_detail_id Int
      )
AS

delete from traffic_detail_audiences where traffic_detail_id = @traffic_detail_id

delete from traffic_details_proposal_details_map where traffic_detail_id = @traffic_detail_id

delete from traffic_details where id = @traffic_detail_id

