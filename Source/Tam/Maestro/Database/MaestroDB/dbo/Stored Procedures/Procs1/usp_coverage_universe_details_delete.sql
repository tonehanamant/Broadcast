CREATE PROCEDURE usp_coverage_universe_details_delete
(
	@coverage_universe_id		Int,
	@network_id		Int,
	@topography_id		Int)
AS
DELETE FROM
	coverage_universe_details
WHERE
	coverage_universe_id = @coverage_universe_id
 AND
	network_id = @network_id
 AND
	topography_id = @topography_id
