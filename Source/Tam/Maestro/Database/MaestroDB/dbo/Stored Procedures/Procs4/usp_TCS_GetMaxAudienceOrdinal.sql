

CREATE Procedure [dbo].[usp_TCS_GetMaxAudienceOrdinal]
      (
            @traffic_id Int
      )
AS

select max(ordinal) from traffic_audiences (NOLOCK) where traffic_id = @traffic_id

