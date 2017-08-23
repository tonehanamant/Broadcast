CREATE PROCEDURE usp_topography_zones_insert
(
	@topography_id		Int,
	@zone_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO topography_zones
(
	topography_id,
	zone_id,
	include,
	effective_date
)
VALUES
(
	@topography_id,
	@zone_id,
	@include,
	@effective_date
)

