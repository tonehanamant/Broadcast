CREATE PROCEDURE usp_topography_systems_update
(
	@topography_id		Int,
	@system_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
UPDATE topography_systems SET
	include = @include,
	effective_date = @effective_date
WHERE
	topography_id = @topography_id AND
	system_id = @system_id
