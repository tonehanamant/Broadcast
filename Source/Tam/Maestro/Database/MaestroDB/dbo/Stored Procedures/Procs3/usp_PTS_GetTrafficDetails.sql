CREATE PROCEDURE [dbo].[usp_PTS_GetTrafficDetails]
(
@tid as int
)
AS

SELECT traffic_details.id from traffic_details 
where traffic_details.traffic_id = @tid
