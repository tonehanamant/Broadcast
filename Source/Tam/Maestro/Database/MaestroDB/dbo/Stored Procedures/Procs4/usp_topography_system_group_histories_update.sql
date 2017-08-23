CREATE PROCEDURE usp_topography_system_group_histories_update
(
	@topography_id		Int,
	@system_group_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
UPDATE topography_system_group_histories SET
	include = @include,
	end_date = @end_date
WHERE
	topography_id = @topography_id AND
	system_group_id = @system_group_id AND
	start_date = @start_date
