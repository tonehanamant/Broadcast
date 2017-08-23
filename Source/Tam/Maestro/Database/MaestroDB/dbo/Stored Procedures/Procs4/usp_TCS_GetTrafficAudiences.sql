
CREATE Procedure [dbo].[usp_TCS_GetTrafficAudiences]
	(
		@id Int
	)
AS
SELECT traffic_id, audience_id, ordinal, universe
from traffic_audiences (NOLOCK) where
traffic_id = @id

