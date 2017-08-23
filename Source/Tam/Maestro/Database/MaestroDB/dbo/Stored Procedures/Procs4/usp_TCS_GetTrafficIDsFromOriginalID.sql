


CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficIDsFromOriginalID]
(
	@original_traffic_id as int
)

AS

SELECT 
	ID 
FROM 
	TRAFFIC (NOLOCK) 
WHERE 
	original_traffic_id = @original_traffic_id;
