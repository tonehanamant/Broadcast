CREATE PROCEDURE usp_topography_states_update
(
	@topography_id		Int,
	@state_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
UPDATE topography_states SET
	include = @include,
	effective_date = @effective_date
WHERE
	topography_id = @topography_id AND
	state_id = @state_id
