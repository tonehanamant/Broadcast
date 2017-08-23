CREATE PROCEDURE usp_topography_system_histories_insert
(
	@topography_id		Int,
	@system_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
INSERT INTO topography_system_histories
(
	topography_id,
	system_id,
	start_date,
	include,
	end_date
)
VALUES
(
	@topography_id,
	@system_id,
	@start_date,
	@include,
	@end_date
)

