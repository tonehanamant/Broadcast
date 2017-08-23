CREATE PROCEDURE [dbo].[usp_PTS_GetNetworkIDs]
( @tid as int)

AS

SELECT distinct traffic_details.network_id from 
traffic_details WHERE traffic_details.traffic_id = @tid 
ORDER BY network_id;
