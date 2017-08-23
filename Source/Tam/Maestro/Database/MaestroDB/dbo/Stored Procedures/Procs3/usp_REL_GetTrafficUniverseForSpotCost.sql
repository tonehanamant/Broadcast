

CREATE PROCEDURE [dbo].[usp_REL_GetTrafficUniverseForSpotCost]
(
      @traffic_detail_id Int,
      @audience_id int,
      @topography_id int
)
AS

select 
      dbo.GetTrafficDetailCoverageUniverse(@traffic_detail_id, @audience_id, @topography_id) AS Traf_Prim_Universe
