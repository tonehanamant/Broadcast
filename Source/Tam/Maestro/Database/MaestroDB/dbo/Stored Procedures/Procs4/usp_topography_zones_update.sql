CREATE PROCEDURE usp_topography_zones_update
(
	@topography_id		Int,
	@zone_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
UPDATE topography_zones SET
	include = @include,
	effective_date = @effective_date
WHERE
	topography_id = @topography_id AND
	zone_id = @zone_id
