CREATE PROCEDURE usp_coverage_universe_details_insert
(
	@coverage_universe_id		Int,
	@network_id		Int,
	@topography_id		Int,
	@universe		Float
)
AS
INSERT INTO coverage_universe_details
(
	coverage_universe_id,
	network_id,
	topography_id,
	universe
)
VALUES
(
	@coverage_universe_id,
	@network_id,
	@topography_id,
	@universe
)

