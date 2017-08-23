CREATE PROCEDURE usp_topography_zone_histories_update
(
	@topography_id		Int,
	@zone_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
UPDATE topography_zone_histories SET
	include = @include,
	end_date = @end_date
WHERE
	topography_id = @topography_id AND
	zone_id = @zone_id AND
	start_date = @start_date
