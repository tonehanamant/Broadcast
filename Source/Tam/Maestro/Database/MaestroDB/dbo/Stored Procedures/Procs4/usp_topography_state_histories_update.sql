CREATE PROCEDURE usp_topography_state_histories_update
(
	@topography_id		Int,
	@state_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
UPDATE topography_state_histories SET
	include = @include,
	end_date = @end_date
WHERE
	topography_id = @topography_id AND
	state_id = @state_id AND
	start_date = @start_date
