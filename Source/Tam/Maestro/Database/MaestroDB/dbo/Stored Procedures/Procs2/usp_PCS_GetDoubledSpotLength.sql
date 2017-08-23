CREATE PROCEDURE [dbo].[usp_PCS_GetDoubledSpotLength]
(
      @spot_length_id as int
)

AS

SELECT 
	spot_lengths.id 
from 
	spot_lengths WITH (NOLOCK)
where 
	length = 
		(select 2*length from spot_lengths where spot_lengths.id = @spot_length_id)
