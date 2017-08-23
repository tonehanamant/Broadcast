CREATE PROCEDURE usp_topography_states_insert
(
	@topography_id		Int,
	@state_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO topography_states
(
	topography_id,
	state_id,
	include,
	effective_date
)
VALUES
(
	@topography_id,
	@state_id,
	@include,
	@effective_date
)

