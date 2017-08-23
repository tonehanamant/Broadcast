

CREATE PROCEDURE [dbo].[usp_TCS_GetSpotLengthMultiplier]

(

      @length int

)

AS

select delivery_multiplier from spot_lengths (NOLOCK) where length = @length
