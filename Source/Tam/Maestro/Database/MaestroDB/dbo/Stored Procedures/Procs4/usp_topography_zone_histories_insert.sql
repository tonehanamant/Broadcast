CREATE PROCEDURE usp_topography_zone_histories_insert
(
	@topography_id		Int,
	@zone_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
INSERT INTO topography_zone_histories
(
	topography_id,
	zone_id,
	start_date,
	include,
	end_date
)
VALUES
(
	@topography_id,
	@zone_id,
	@start_date,
	@include,
	@end_date
)

