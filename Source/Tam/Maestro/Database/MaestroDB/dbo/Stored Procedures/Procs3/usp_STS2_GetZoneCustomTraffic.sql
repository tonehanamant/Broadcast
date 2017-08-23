	
CREATE PROCEDURE [dbo].[usp_STS2_GetZoneCustomTraffic]
(
	@zone_id int
)
AS
BEGIN

SELECT [zone_id]
      ,[traffic_factor]
      ,[effective_date]
FROM [dbo].[zone_custom_traffic]
WHERE [zone_id] = @zone_id

END