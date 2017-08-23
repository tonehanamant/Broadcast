CREATE PROCEDURE usp_topography_system_groups_update
(
	@topography_id		Int,
	@system_group_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
UPDATE topography_system_groups SET
	include = @include,
	effective_date = @effective_date
WHERE
	topography_id = @topography_id AND
	system_group_id = @system_group_id
