
Create Procedure [dbo].[usp_REL_GetSpotLength]
(
	@length int
)
AS
BEGIN

	select 
		id,
		length,
		delivery_multiplier,
		order_by,
		is_default
	from
		spot_lengths (NOLOCK)
	where
		spot_lengths.length = @length
END
