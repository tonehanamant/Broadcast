CREATE PROCEDURE usp_topography_system_histories_update
(
	@topography_id		Int,
	@system_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
UPDATE topography_system_histories SET
	include = @include,
	end_date = @end_date
WHERE
	topography_id = @topography_id AND
	system_id = @system_id AND
	start_date = @start_date
