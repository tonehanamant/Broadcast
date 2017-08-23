CREATE PROCEDURE usp_topography_state_histories_insert
(
	@topography_id		Int,
	@state_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
INSERT INTO topography_state_histories
(
	topography_id,
	state_id,
	start_date,
	include,
	end_date
)
VALUES
(
	@topography_id,
	@state_id,
	@start_date,
	@include,
	@end_date
)

