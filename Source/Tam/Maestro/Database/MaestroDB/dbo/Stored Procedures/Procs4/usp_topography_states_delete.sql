CREATE PROCEDURE usp_topography_states_delete
(
	@topography_id		Int,
	@state_id		Int
)
AS
DELETE FROM topography_states WHERE topography_id=@topography_id AND state_id=@state_id
