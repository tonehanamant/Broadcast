CREATE PROCEDURE usp_coverage_universe_details_update
(
	@coverage_universe_id		Int,
	@network_id		Int,
	@topography_id		Int,
	@universe		Float
)
AS
UPDATE coverage_universe_details SET
	universe = @universe
WHERE
	coverage_universe_id = @coverage_universe_id AND
	network_id = @network_id AND
	topography_id = @topography_id
